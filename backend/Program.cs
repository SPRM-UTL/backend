using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios básicos al contenedor
builder.Services.AddControllersWithViews();

// 2. Configurar la política estricta de CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// 3. Configurar el contexto de Base de Datos para MySQL
builder.Services.AddDbContext<PruebaaspContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("conexion"),
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.7-mysql")
    ));

var app = builder.Build();

// ==========================================
// CONFIGURACIÓN DEL PIPELINE DE PETICIONES (MIDDLEWARES)
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirección de HTTP a HTTPS y enrutamiento básico
app.UseHttpsRedirection();
app.UseRouting();

// 🔥 CRÍTICO: CORS debe ejecutarse inmediatamente después de Routing y ANTES de cualquier Middleware de Autorización o petición.
app.UseCors("AngularPolicy");

// Middlewares personalizados y seguridad
app.UseMiddleware<RequestMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

// Configuración de rutas de controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Encender la aplicación
app.Run();