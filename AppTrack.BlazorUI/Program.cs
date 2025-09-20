using AppTrack.BlazorUI;
using AppTrack.BlazorUI.Providers;
using AppTrack.BlazorUI.TokenStorage;
using AppTrack.Frontend.ApiService;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddApiServiceServices();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ITokenStorage, BlazorTokenStorage>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();

builder.Services.AddAutoMapper(cfg =>  
{ 
    cfg.AddMaps(Assembly.GetExecutingAssembly());
});

await builder.Build().RunAsync();
