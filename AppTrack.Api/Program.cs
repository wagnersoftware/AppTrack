using AppTrack.Api.Helper;
using AppTrack.Api.Middleware;
using AppTrack.Application;
using AppTrack.Identity;
using AppTrack.Infrastructure;
using AppTrack.Infrastructure.ApplicationTextGeneration;
using AppTrack.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistanceServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

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


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // JWT Bearer Auth
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

// Kestrel configuration
builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false; //don't expose the framework type in the response header
});

// avoid captive dependency problem (eg. a scoped dependency injected into a singleton) for all environments
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

await MigrationsHelper.TryApplyDatabaseMigrations(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("All");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "For integration testing")]
public partial class Program { }

