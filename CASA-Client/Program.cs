using CASA_Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Project services
builder.Services.AddSingleton<ServerControlService>();
builder.Services.AddScoped<NetworkingService>();
builder.Services.AddScoped<PacketLogService>();
builder.Services.AddScoped<ThemeState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<CASA_Client.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
