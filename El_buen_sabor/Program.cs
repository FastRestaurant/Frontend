using Blazored.LocalStorage;
using El_buen_sabor.Components;
using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<OrderRealtimeService>();
builder.Services.AddScoped<KitchenRealtimeService>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IFacturationService, FacturationService>();

builder.Services.AddScoped(sp =>
{
    var authBaseUrl = builder.Configuration["http://localhost:5155/"] ?? "http://localhost:5155/";
    return new HttpClient
    {
        BaseAddress = new Uri(authBaseUrl)
    };
});
builder.Services.AddHttpClient<MenuService>(client =>
{
    var menuBaseUrl = builder.Configuration["http://localhost:5127/"] ?? "http://localhost:5127/";
    client.BaseAddress = new Uri(menuBaseUrl);
});
builder.Services.AddHttpClient<MenuCatalogService>(client =>
{
    var menuBaseUrl = builder.Configuration["http://localhost:5127/"] ?? "http://localhost:5127/";
    client.BaseAddress = new Uri(menuBaseUrl);
});
builder.Services.AddHttpClient<ITableService, TableService>(client =>
{
    var ordersBaseUrl = builder.Configuration["http://localhost:5231/"] ?? "http://localhost:5231/";
    client.BaseAddress = new Uri(ordersBaseUrl);
});
builder.Services.AddHttpClient<TablesService>(client =>
{
    var ordersBaseUrl = builder.Configuration["http://localhost:5231/"] ?? "http://localhost:5231/";
    client.BaseAddress = new Uri(ordersBaseUrl);
});
builder.Services.AddHttpClient<KitchenService>(client =>
{
    var kitchenBaseUrl = builder.Configuration["http://localhost:5207/"] ?? "http://localhost:5207/";
    client.BaseAddress = new Uri(kitchenBaseUrl);
});
builder.Services.AddHttpClient<StockService>(client =>
{
    var stockBaseUrl = builder.Configuration["http://localhost:5093/"] ?? "http://localhost:5093/";
    client.BaseAddress = new Uri(stockBaseUrl);
});

builder.Services.AddHttpClient<IFacturationService, FacturationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5231/");
});

builder.Services.AddBlazoredLocalStorage();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
