using AppTrack.BlazorUi.Components;
using AppTrack.BlazorUi.TokenStorage;
using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.Frontend.Models.ModelValidator;
using AppTrack.Frontend.Models.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<ITokenStorage, BlazorTokenStorage>();
builder.Services.AddApiServiceServices(builder.Configuration);

builder.Services.AddTransient<IValidator<LoginModel>, LoginModelValidator>();
builder.Services.AddTransient<IValidator<RegistrationModel>, RegistrationModelValidator>();
builder.Services.AddTransient(typeof(IModelValidator<>), typeof(ModelValidator<>));

await builder.Build().RunAsync();
