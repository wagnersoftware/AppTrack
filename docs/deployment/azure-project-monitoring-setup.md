# Azure Setup Guide: Project Scraping & Monitoring

This guide covers all Azure resources required to run the project scraping, keyword matching, and email notification pipeline in production.

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
          │  reads IsNotified=false matches, sends email via SendGrid
          └─► marks IsNotified=true

[Timer Trigger - weekly]
  CleanupFunction
          └─► deletes ScrapedProjects, ProcessedProjectItems, UserProjectMatches older than 60 days
```

## Prerequisites

- Azure CLI installed (`az --version`)
- Logged in: `az login`
- Azure subscription available
- SendGrid account with a verified sender address (already used by the API)
- Existing **Azure SQL Database** and **Azure App Service** for the AppTrack API (migrations run automatically on API startup)

---

## Step 1: Resource Group

Use the same resource group as the existing AppTrack API, or create a dedicated one:

```bash
az group create \
  --name rg-apptrack-prod \
  --location germanywestcentral
```

---

## Step 2: Azure Storage Account

Required by Azure Functions (state, triggers, logs).

```bash
az storage account create \
  --name stapptrackprod \
  --resource-group rg-apptrack-prod \
  --location germanywestcentral \
  --sku Standard_LRS \
  --kind StorageV2
```

Save the connection string — needed later:

```bash
az storage account show-connection-string \
  --name stapptrackprod \
  --resource-group rg-apptrack-prod \
  --query connectionString -o tsv
```

---

## Step 3: Azure Service Bus

### 3a — Namespace

```bash
az servicebus namespace create \
  --name sb-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --location germanywestcentral \
  --sku Basic
```

> **SKU note:** Basic supports queues. If you ever need topics/subscriptions, upgrade to Standard.

### 3b — Queue

```bash
az servicebus queue create \
  --name scraping-completed \
  --namespace-name sb-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --max-delivery-count 3 \
  --default-message-time-to-live P1D
```

> `--max-delivery-count 3` — after 3 failed deliveries the message goes to the dead-letter queue instead of retrying indefinitely.

### 3c — Connection String

```bash
az servicebus namespace authorization-rule keys list \
  --namespace-name sb-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString -o tsv
```

Save this value — it becomes `ServiceBusConnection` in the Function App settings.

---

## Step 4: Function App

```bash
az functionapp create \
  --name func-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --storage-account stapptrackprod \
  --consumption-plan-location germanywestcentral \
  --runtime dotnet-isolated \
  --runtime-version 10 \
  --functions-version 4 \
  --os-type Windows
```

> The Consumption Plan means you pay per execution — ideal for periodic scraping jobs.

---

## Step 5: Configure Application Settings

These settings replace `local.settings.json` in production. Set them all at once:

```bash
az functionapp config appsettings set \
  --name func-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --settings \
    "AzureWebJobsStorage=<storage-connection-string-from-step-2>" \
    "ConnectionStrings__AppTrackConnectionString=<azure-sql-connection-string>" \
    "ServiceBusConnection=<service-bus-connection-string-from-step-3c>" \
    "ScrapingCompletedQueueName=scraping-completed" \
    "ScrapeSchedule=0 0 6 * * *" \
    "NotificationSchedule=0 30 6 * * *" \
    "CleanupSchedule=0 0 3 * * 0" \
    "EmailSettings__FromAddress=<your-verified-sender@domain.com>" \
    "EmailSettings__ApiKey=<sendgrid-api-key>"
```

### Schedule Reference (NCRONTAB)

| Setting | Example Value | Meaning |
|---|---|---|
| `ScrapeSchedule` | `0 0 6 * * *` | Every day at 06:00 UTC |
| `NotificationSchedule` | `0 30 6 * * *` | Every day at 06:30 UTC (30 min after scrape) |
| `CleanupSchedule` | `0 0 3 * * 0` | Every Sunday at 03:00 UTC |

> Set `NotificationSchedule` **after** `ScrapeSchedule` to give matching time to complete. 30 minutes is a safe buffer.

### SQL Connection String Format (Azure SQL)

```
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=AppTrack;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## Step 6: Deploy the Functions

### Option A — GitHub Actions (recommended)

Add a workflow that builds and publishes `AppTrack.Functions` on push to `main`:

```yaml
- name: Build
  run: dotnet publish AppTrack.Functions/AppTrack.Functions.csproj -c Release -o ./publish

- name: Deploy to Azure Functions
  uses: Azure/functions-action@v1
  with:
    app-name: func-apptrack-prod
    package: ./publish
```

### Option B — Azure CLI (manual)

```bash
dotnet publish AppTrack.Functions/AppTrack.Functions.csproj \
  --configuration Release \
  --output ./publish

cd publish && zip -r ../functions.zip . && cd ..

az functionapp deployment source config-zip \
  --name func-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --src functions.zip
```

---

## Step 7: Verify

### Check Functions are registered

```bash
az functionapp function list \
  --name func-apptrack-prod \
  --resource-group rg-apptrack-prod \
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
  --resource-group rg-apptrack-prod \
  --query "masterKey" -o tsv
```

### Check Service Bus Queue

After triggering scraping, verify a message arrived in the queue:

```bash
az servicebus queue show \
  --name scraping-completed \
  --namespace-name sb-apptrack-prod \
  --resource-group rg-apptrack-prod \
  --query "countDetails" -o table
```

`MatchProjectsFunction` will consume the message automatically (Service Bus trigger). If `activeMessageCount` stays > 0, check the Function App logs.

### Monitor Logs

```bash
az webapp log tail \
  --name func-apptrack-prod \
  --resource-group rg-apptrack-prod
```

---

## Settings Summary

| Setting | Source | Description |
|---|---|---|
| `AzureWebJobsStorage` | Step 2 | Storage Account connection string |
| `ConnectionStrings__AppTrackConnectionString` | Existing Azure SQL | Main database |
| `ServiceBusConnection` | Step 3c | Service Bus connection string |
| `ScrapingCompletedQueueName` | Fixed: `scraping-completed` | Must match queue name from Step 3b |
| `ScrapeSchedule` | Your preference | NCRONTAB — when to scrape |
| `NotificationSchedule` | Your preference | NCRONTAB — when to send emails |
| `CleanupSchedule` | Your preference | NCRONTAB — when to run cleanup (e.g. weekly) |
| `EmailSettings__FromAddress` | SendGrid | Verified sender address |
| `EmailSettings__ApiKey` | SendGrid | API key |
