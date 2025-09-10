using ApexCharts;
using Blazored.LocalStorage;
using NarrativeSimulator.Components;
using NarrativeSimulator.Core.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddNarrativeServices();
builder.Services.AddSentinoClient(opts =>
{
    opts.BaseUrl = "https://sentino.p.rapidapi.com";
    opts.ApiKey = builder.Configuration["Sentino:ApiKey"] ?? string.Empty;
});
builder.Services.AddSymantoClient(opts =>
{
    opts.ApiKey = builder.Configuration["Sentino:ApiKey"] ?? string.Empty;
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddApexCharts(o =>
{
    o.GlobalOptions = new ApexChartBaseOptions() { Tooltip = new Tooltip() { CssClass = "apex-tooltip" } };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
