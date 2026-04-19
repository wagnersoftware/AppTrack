using AppTrack.Api.Helper;
using AppTrack.Api.Logging;
using AppTrack.Api.Middleware;
using AppTrack.Application;
using AppTrack.Infrastructure;
using AppTrack.Infrastructure.AiTextGeneration;
using AppTrack.Persistance;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

// Bootstrap logger — active only until the host is built.
// Replaced by the full Serilog configuration below via UseSerilog().
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, loggerConfig) =>
    {
        var env = context.HostingEnvironment.EnvironmentName;
        var appInsightsConnStr =
            context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.FromLogContext()
            .Enrich.With<CorrelationIdEnricher>()
            .Destructure.With<PiiDestructuringPolicy>()
            // Console: all environments except Test (suppressed in Test to keep test output clean)
            .WriteTo.Conditional(_ => env != "Test",
                wt => wt.Console())
            // File: dev only, rolling daily JSON, 7-day retention
            .WriteTo.Conditional(_ => env == "Development",
                wt => wt.File(
                    new CompactJsonFormatter(),
                    "logs/apptrack-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7))
            // Application Insights: prod only, only when connection string is present
            .WriteTo.Conditional(
                _ => env == "Production" && appInsightsConnStr is not null,
                wt => wt.ApplicationInsights(
                    appInsightsConnStr!,
                    TelemetryConverter.Traces));
    });
    // ─────────────────────────────────────────────────────────────────────

    if (!builder.Environment.IsDevelopment())
    {
        var keyVaultUri = builder.Configuration["KeyVaultUri"];
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
        }
    }

    builder.Services.AddHealthChecks();

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddPersistanceServices(builder.Configuration);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(
            jwtBearerOptions =>
            {
                // Prevent ASP.NET Core from remapping claim names (e.g. "sub" → ClaimTypes.NameIdentifier).
                jwtBearerOptions.MapInboundClaims = false;

                // Explicitly set the CIAM metadata endpoint — required because ciamlogin.com
                // uses a different discovery path than standard AAD.
                var instance = builder.Configuration["AzureAd:Instance"]?.TrimEnd('/');
                var tenantId = builder.Configuration["AzureAd:TenantId"];
                jwtBearerOptions.MetadataAddress =
                    $"{instance}/{tenantId}/v2.0/.well-known/openid-configuration";
            },
            microsoftIdentityOptions =>
            {
                builder.Configuration.GetSection("AzureAd").Bind(microsoftIdentityOptions);
            });

    builder.Services.AddControllers();

    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("All", policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });

    builder.Services.AddOptions<OpenAiOptions>()
        .Bind(builder.Configuration.GetSection("OpenAiSettings"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        c.UseAllOfToExtendReferenceSchemas();
        c.SchemaFilter<AppTrack.Api.Swagger.NullableEnumSchemaFilter>();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Bearer + token value"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    // Global Authorization Policy
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

    // Remove the "Server" header from responses for security hardening
    builder.Services.PostConfigure<KestrelServerOptions>(options =>
    {
        options.AddServerHeader = false;
    });

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateOnBuild = true;
    });

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────
    // UseSerilogRequestLogging must be registered FIRST (outermost wrapper).
    // It calls next(), waits for the full pipeline to complete, then logs
    // the HTTP summary — so it observes the final status code set by
    // ExceptionMiddleware below.
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var userId = httpContext.User.FindFirst("sub")?.Value;
            if (userId is not null)
                diagnosticContext.Set("UserId", userId);
            // Note: "Environment" is NOT set here — it is already attached to
            // every log event globally via .Enrich.WithProperty("Environment", ...)
            // in UseSerilog above. Setting it here again would be redundant.
        };
    });
    app.UseMiddleware<ExceptionMiddleware>();
    // ─────────────────────────────────────────────────────────────────────

    await MigrationsHelper.TryApplyDatabaseMigrations(app);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("All");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "For integration testing")]
public partial class Program { }
