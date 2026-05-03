# Azure Setup Guide: Project Scraping & Monitoring

This guide covers all Azure resources required to run the project scraping, keyword matching, and email notification pipeline in production. All services authenticate via **Managed Identity** — no connection strings or API keys are stored in application settings.

## Architecture Overview

```
[Timer Trigger]
  ScrapePortalsFunction
      │
      └─► Azure Service Bus Queue (scraping-completed)
                │
                ▼
      MatchProjectsFunction          ← Service Bus Trigger
          │  creates UserProjectMatch + JobApplication per user
          │
[Timer Trigger]
  SendNotificationsFunction
          │  reads IsNotified=false matches, sends email via Azure Communication Services
          └─► marks IsNotified=true

[Timer Trigger - weekly]
  CleanupFunction
          └─► deletes ScrapedProjects, ProcessedProjectItems, UserProjectMatches older than 60 days
```

## Prerequisites

- Azure CLI installed (`az --version`)
- Logged in: `az login`
- Resource group `rg-apptrack` already exists
- Storage account `apptrackstorage` already exists in `rg-apptrack`
- Existing **Azure SQL Database** and **Azure App Service** for the AppTrack API

---

## Step 1: Azure Service Bus

### 1a — Namespace

```bash
az servicebus namespace create \
  --name sb-apptrack-prod \
  --resource-group rg-apptrack \
  --location germanywestcentral \
  --sku Basic
```

> **SKU note:** Basic supports queues. If you ever need topics/subscriptions, upgrade to Standard.

### 1b — Queue

```bash
az servicebus queue create \
  --name scraping-completed \
  --namespace-name sb-apptrack-prod \
  --resource-group rg-apptrack \
  --max-delivery-count 3 \
  --default-message-time-to-live P1D
```

> `--max-delivery-count 3` — after 3 failed deliveries the message goes to the dead-letter queue instead of retrying indefinitely.

---

## Step 2: Azure Communication Services Email

### 2a — Create Communication Services Resource

```bash
az communication create \
  --name acs-apptrack-prod \
  --resource-group rg-apptrack \
  --location global \
  --data-location germany
```

> `--location global` is required for ACS resources (control plane is global); `--data-location` sets where data is stored at rest.

### 2b — Create Email Services Resource

ACS Email requires a separate `EmailServices` resource (distinct from `CommunicationServices`):

```bash
az communication email create \
  --name email-apptrack-prod \
  --resource-group rg-apptrack \
  --location global \
  --data-location germany
```

### 2c — Add an Email Domain

Use an Azure-managed domain (easiest, no DNS setup):

```bash
az communication email domain create \
  --name AzureManagedDomain \
  --email-service-name email-apptrack-prod \
  --resource-group rg-apptrack \
  --location global \
  --domain-management AzureManaged
```

This provisions a `<guid>.azurecomm.net` domain with a ready-to-use sender address.

To use a **custom domain** instead, replace `--domain-management AzureManaged` with `CustomerManaged` and verify ownership via DNS TXT record (see [ACS docs](https://learn.microsoft.com/azure/communication-services/quickstarts/email/add-custom-verified-email-domain)).

### 2d — Link Email Domain to Communication Services

```bash
az communication update \
  --name acs-apptrack-prod \
  --resource-group rg-apptrack \
  --linked-domains "/subscriptions/<subscription-id>/resourceGroups/rg-apptrack/providers/Microsoft.Communication/emailServices/email-apptrack-prod/domains/AzureManagedDomain"
```

---

## Step 3: Function App

### 3a — Create

```bash
az functionapp create \
  --name func-apptrack-prod \
  --resource-group rg-apptrack \
  --storage-account apptrackstorage \
  --consumption-plan-location germanywestcentral \
  --runtime dotnet-isolated \
  --runtime-version 10 \
  --functions-version 4 \
  --os-type Windows
```

> The Consumption Plan means you pay per execution — ideal for periodic scraping jobs.

### 3b — Enable System-Assigned Managed Identity

```bash
az functionapp identity assign \
  --name func-apptrack-prod \
  --resource-group rg-apptrack
```

Note the `principalId` from the output — needed for all role assignments below.

### 3c — Grant Storage Permissions

Azure Functions uses the storage account internally (state, triggers, logs). Three roles are required:

```bash
ST_SCOPE=$(az storage account show \
  --name apptrackstorage \
  --resource-group rg-apptrack \
  --query id -o tsv)

az role assignment create --assignee <principalId> --role "Storage Blob Data Owner"          --scope $ST_SCOPE
az role assignment create --assignee <principalId> --role "Storage Queue Data Contributor"   --scope $ST_SCOPE
az role assignment create --assignee <principalId> --role "Storage Table Data Contributor"   --scope $ST_SCOPE
```

### 3d — Grant Service Bus Permissions

```bash
SB_SCOPE=$(az servicebus namespace show \
  --name sb-apptrack-prod \
  --resource-group rg-apptrack \
  --query id -o tsv)

# Send messages (ScrapePortalsFunction → publisher)
az role assignment create --assignee <principalId> --role "Azure Service Bus Data Sender"   --scope $SB_SCOPE

# Receive messages (MatchProjectsFunction trigger)
az role assignment create --assignee <principalId> --role "Azure Service Bus Data Receiver" --scope $SB_SCOPE
```

### 3e — Grant ACS Email Permission

```bash
az role assignment create \
  --assignee <principalId> \
  --role "Contributor" \
  --scope $(az communication show \
      --name acs-apptrack-prod \
      --resource-group rg-apptrack \
      --query id -o tsv)
```

### 3f — Grant Azure SQL Permission

Create a contained database user for the Function App's managed identity. Connect to the `AppTrack` database (via Azure Portal Query Editor, SSMS, or sqlcmd) and run:

```sql
CREATE USER [func-apptrack-prod] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [func-apptrack-prod];
ALTER ROLE db_datawriter ADD MEMBER [func-apptrack-prod];
```

---

## Step 4: Configure Application Settings

No secrets — all authentication via Managed Identity:

```bash
az functionapp config appsettings set \
  --name func-apptrack-prod \
  --resource-group rg-apptrack \
  --settings \
    "AzureWebJobsStorage__accountName=apptrackstorage" \
    "ConnectionStrings__AppTrackConnectionString=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=AppTrack;Authentication=Active Directory Default;MultipleActiveResultSets=True;Encrypt=True;Connection Timeout=30;" \
    "ServiceBusConnection__fullyQualifiedNamespace=sb-apptrack-prod.servicebus.windows.net" \
    "ScrapingCompletedQueueName=scraping-completed" \
    "ScrapeSchedule=0 0 6 * * *" \
    "NotificationSchedule=0 30 6 * * *" \
    "CleanupSchedule=0 0 3 * * 0" \
    "EmailSettings__Endpoint=https://acs-apptrack-prod.communication.azure.com" \
    "EmailSettings__FromAddress=<sender@your-acs-domain.azurecomm.net>"
```

### Schedule Reference (NCRONTAB)

| Setting | Example Value | Meaning |
|---|---|---|
| `ScrapeSchedule` | `0 0 6 * * *` | Every day at 06:00 UTC |
| `NotificationSchedule` | `0 30 6 * * *` | Every day at 06:30 UTC (30 min after scrape) |
| `CleanupSchedule` | `0 0 3 * * 0` | Every Sunday at 03:00 UTC |

> Set `NotificationSchedule` **after** `ScrapeSchedule` to give matching time to complete. 30 minutes is a safe buffer.

---

## Step 5: Deploy the Functions

### Option A — GitHub Actions (recommended)

The workflow file `.github/workflows/apptrack-functions.yml` is already committed. It deploys automatically on every push to `main` using OIDC (Workload Identity Federation) — no stored secrets needed.

#### 5a — Create App Registration for OIDC

```bash
az ad app create --display-name "github-apptrack-functions"
```

Note the `appId` from the output.

Create a service principal for it:

```bash
az ad sp create --id <appId>
```

Note the service principal `id` (object ID) from the output.

Assign the `Contributor` role on the Function App:

```bash
az role assignment create \
  --assignee <service-principal-id> \
  --role "Contributor" \
  --scope $(az functionapp show \
      --name func-apptrack-prod \
      --resource-group rg-apptrack \
      --query id -o tsv)
```

#### 5b — Add Federated Credential (GitHub OIDC)

```bash
az ad app federated-credential create \
  --id <appId> \
  --parameters '{
    "name": "github-apptrack-functions-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<your-github-org>/<your-repo>:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

Replace `<your-github-org>/<your-repo>` with your actual GitHub repository path (e.g. `daniel/AppTrack`).

#### 5c — Add GitHub Repository Variables

In GitHub: **Repository → Settings → Secrets and variables → Actions → Variables**

| Variable | Value |
|---|---|
| `FUNC_APPTRACK_CLIENT_ID` | `appId` from Step 5a |
| `FUNC_APPTRACK_TENANT_ID` | `az account show --query tenantId -o tsv` |
| `FUNC_APPTRACK_SUBSCRIPTION_ID` | `az account show --query id -o tsv` |

### Option B — Azure CLI (manual)

```bash
dotnet publish AppTrack.Functions/AppTrack.Functions.csproj \
  --configuration Release \
  --output ./publish

cd publish && zip -r ../functions.zip . && cd ..

az functionapp deployment source config-zip \
  --name func-apptrack-prod \
  --resource-group rg-apptrack \
  --src functions.zip
```

---

## Step 6: Verify

### Check Functions are registered

```bash
az functionapp function list \
  --name func-apptrack-prod \
  --resource-group rg-apptrack \
  --query "[].name" -o tsv
```

Expected output:
```
ScrapePortalsFunction
MatchProjectsFunction
SendNotificationsFunction
CleanupFunction
```

### Trigger scraping manually (one-time test)

```bash
az rest --method post \
  --uri "https://func-apptrack-prod.azurewebsites.net/admin/functions/ScrapePortalsFunction" \
  --headers "x-functions-key=<master-key>" \
  --body "{}"
```

Get the master key:

```bash
az functionapp keys list \
  --name func-apptrack-prod \
  --resource-group rg-apptrack \
  --query "masterKey" -o tsv
```

### Check Service Bus Queue

```bash
az servicebus queue show \
  --name scraping-completed \
  --namespace-name sb-apptrack-prod \
  --resource-group rg-apptrack \
  --query "countDetails" -o table
```

`MatchProjectsFunction` will consume the message automatically (Service Bus trigger). If `activeMessageCount` stays > 0, check the Function App logs.

### Monitor Logs

```bash
az webapp log tail \
  --name func-apptrack-prod \
  --resource-group rg-apptrack
```

---

## Settings Summary

| Setting | Value | Description |
|---|---|---|
| `AzureWebJobsStorage__accountName` | `apptrackstorage` | Storage account (managed identity auth) |
| `ConnectionStrings__AppTrackConnectionString` | See Step 4 | SQL Server with managed identity auth |
| `ServiceBusConnection__fullyQualifiedNamespace` | `sb-apptrack-prod.servicebus.windows.net` | Service Bus namespace (managed identity auth) |
| `ScrapingCompletedQueueName` | `scraping-completed` | Must match queue name from Step 1b |
| `ScrapeSchedule` | Your preference | NCRONTAB — when to scrape |
| `NotificationSchedule` | Your preference | NCRONTAB — when to send emails |
| `CleanupSchedule` | Your preference | NCRONTAB — when to run cleanup |
| `EmailSettings__Endpoint` | `https://acs-apptrack-prod.communication.azure.com` | ACS resource endpoint (managed identity auth) |
| `EmailSettings__FromAddress` | `<sender@your-acs-domain.azurecomm.net>` | Sender address from ACS domain |
