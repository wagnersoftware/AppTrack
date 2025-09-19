using AppTrack.BlazorUI;
using AppTrack.BlazorUI.Contracts;
using AppTrack.BlazorUI.Providers;
using AppTrack.BlazorUI.Services;
using AppTrack.BlazorUI.Services.Base;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient<IClient, Client>(client => client.BaseAddress = new Uri("https://localhost:7273"));

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();

builder.Services.AddAutoMapper(cfg =>  
{ 
    cfg.AddMaps(Assembly.GetExecutingAssembly());
});

await builder.Build().RunAsync();
