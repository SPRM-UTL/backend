using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Middleware;
using DotNetEnv;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Esp32ConnectionManager>();
builder.Services.AddSingleton<Esp32MessageEventHub>();
builder.Services.AddScoped<Esp32MessageRouter>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var connectionString =
    Environment.GetEnvironmentVariable("CONEXION") ??
    builder.Configuration.GetConnectionString("conexion");

builder.Services.AddDbContext<PruebaaspContext>(options =>
    options.UseMySql(
        connectionString,
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.4.7-mysql"),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AngularPolicy");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseMiddleware<RequestMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
