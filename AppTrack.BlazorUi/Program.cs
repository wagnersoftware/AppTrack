using AppTrack.BlazorUi.Components;
using AppTrack.BlazorUi.Services;
using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.Frontend.Models.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    var apiScope = builder.Configuration["AzureAd:ApiScope"];
    if (!string.IsNullOrEmpty(apiScope))
        options.ProviderOptions.DefaultAccessTokenScopes.Add(apiScope);
});

builder.Services.AddApiServiceServices(builder.Configuration);

builder.Services.AddTransient<IValidator<JobApplicationModel>, JobApplicationModelValidator>();
builder.Services.AddTransient<IValidator<AiSettingsModel>, AiSettingsModelValidator>();
builder.Services.AddTransient<IValidator<PromptParameterModel>, PromptParameterModelValidator>();
builder.Services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));

await builder.Build().RunAsync();
