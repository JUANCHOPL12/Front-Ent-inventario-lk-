using TecnologiaDigitalLK.Components;
using TecnologiaDigitalLK.Services.Interfaces;
using TecnologiaDigitalLK.Services.Mocks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registro de servicios (DEBE IR ANTES DE builder.Build())
builder.Services.AddSingleton<MockDataStore>();
builder.Services.AddSingleton<IUserSessionService, MockUserSessionService>(); // <-- ÚNICO CAMBIO AQUÍ
builder.Services.AddScoped<IInventarioService, MockInventarioService>();
builder.Services.AddScoped<IVentasService, MockVentasService>();
builder.Services.AddScoped<ICierreCajaService, MockCierreCajaService>();
builder.Services.AddScoped<IAdministracionService, MockAdministracionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();