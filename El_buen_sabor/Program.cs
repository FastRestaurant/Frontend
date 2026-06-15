using Blazored.LocalStorage;
using El_buen_sabor.Components;
using El_buen_sabor.Components.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped(sp =>
{
    var authBaseUrl = builder.Configuration["ExternalServices:Auth:BaseUrl"] ?? "https://localhost:7060/";
    return new HttpClient
    {
        BaseAddress = new Uri(authBaseUrl)
    };
});
builder.Services.AddHttpClient<TablesService>(client =>
{
    var ordersBaseUrl = builder.Configuration["ExternalServices:Orders:BaseUrl"] ?? "https://localhost:7100/";
    client.BaseAddress = new Uri(ordersBaseUrl);
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

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
