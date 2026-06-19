using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Middleware;
using DotNetEnv;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Esp32ConnectionManager>();
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
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

        db.Database.Migrate();

        // Seeder dinámico de tipos de aparatos
        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Asistente"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Asistente", icono = "ic_input_add", es_asistente = true, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = null });
        
        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Bocinas"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Bocinas", icono = "speaker", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "SPEAKER,BOCINA,ALTAVOZ,SOUNDBAR,CHARGE,FLIP,XTREME,BOOMBOX,SOUNDLINK" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Audífonos"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Audífonos", icono = "headphones", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "HEADPHONE,HEADSET,EARPHONE,EARBUDS,AUDIFONOS,AUDÍFONOS,AURICULAR,AUT,TWS,BUDS,AIRPOD,WH-,WF-,QC,TUNE,FREEBUDS" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Focos"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Focos", icono = "lightbulb", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "FOCO,LIGHT,BULB,LAMP" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Luces"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Luces", icono = "lamp_floor", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "STRIP,TIRA,LUCES,LED,LAMP" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Ventilador"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Ventilador", icono = "wind", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "FAN,VENTILADOR" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Televisión"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Televisión", icono = "tv_minimal", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, palabras_clave_busqueda = "TV,TELEVISION,SCREEN,DISPLAY" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Sockets Inteligentes"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Sockets Inteligentes", icono = "plug", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, palabras_clave_busqueda = "SOCKET,PLUG,ENCHUFE,SMARTPLUG" });

        db.SaveChanges(); // Guardar nuevas adiciones antes de buscar para actualizar

        // Actualizar iconos y métodos de configuración de aparatos existentes si no tienen
        var tipos = db.AparatoTipos.ToList();
        foreach (var tipo in tipos) {
            // Forzar configuración WiFi-only por defecto a los antiguos
            if (tipo.nombre_tipo != "Sockets Inteligentes") {
                tipo.soporta_bluetooth = false;
                tipo.soporta_wifi = true;
            } else {
                tipo.soporta_bluetooth = true;
                tipo.soporta_wifi = true;
            }

            if (string.IsNullOrEmpty(tipo.icono)) {
                switch (tipo.nombre_tipo) {
                    case "Focos": tipo.icono = "lightbulb"; break;
                    case "Bocinas": tipo.icono = "speaker"; break;
                    case "Audífonos": tipo.icono = "headphones"; break;
                    case "Luces": tipo.icono = "lamp_floor"; break;
                    case "Ventilador": tipo.icono = "wind"; break;
                    case "Televisión": tipo.icono = "tv_minimal"; break;
                    case "Sockets Inteligentes": tipo.icono = "plug"; break;
                    case "Asistente": tipo.icono = "ic_input_add"; break;
                }
            }
        }
        db.SaveChanges();
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error al aplicar migraciones de la base de datos.");
}

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
