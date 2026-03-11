using AppTrack.Api.Helper;
using AppTrack.Api.Middleware;
using AppTrack.Application;
using AppTrack.Infrastructure;
using AppTrack.Infrastructure.ApplicationTextGeneration;
using AppTrack.Persistance;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("All", builder => builder.AllowAnyOrigin()
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

builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "For integration testing")]
public partial class Program { }
