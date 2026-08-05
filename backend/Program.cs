using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Middleware;
using DotNetEnv;
using backend.Services;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Esp32ConnectionManager>();
builder.Services.AddScoped<Esp32MessageRouter>();
builder.Services.AddScoped<Esp32DeviceStateService>();
builder.Services.AddScoped<TuyaLocalService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://frontend-itgu.onrender.com", "https://manordomo-web.onrender.com")
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migrate falló");
    }

    try
    {
        EnsureEsp32Schema(db);
        EnsureConsumoSchema(db);
        EnsureMultiSocketSchema(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "EnsureEsp32Schema falló");
    }
}

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

        // Seeder dinámico de tipos de aparatos
        var tiposDeseados = new List<AparatoTipo>
        {
            new AparatoTipo { sk_aparato_tipo_id = 14, nombre_tipo = "Lampara Inteligente", es_asistente = false, icono = "plug", soporta_wifi = true, soporta_bluetooth = true, palabras_clave_busqueda = "", orden = 2, requiere_vinculacion_bluetooth = true },
            new AparatoTipo { sk_aparato_tipo_id = 15, nombre_tipo = "MultiSocket", es_asistente = false, icono = "lucide_lightbulb", soporta_wifi = true, soporta_bluetooth = true, palabras_clave_busqueda = "", orden = 3, requiere_vinculacion_bluetooth = false },
            new AparatoTipo { sk_aparato_tipo_id = 16, nombre_tipo = "ESP32-CAM", es_asistente = false, icono = "videocam", soporta_wifi = true, soporta_bluetooth = true, palabras_clave_busqueda = "", orden = 1, requiere_vinculacion_bluetooth = false },
            new AparatoTipo { sk_aparato_tipo_id = 17, nombre_tipo = "Socket Generico", es_asistente = false, icono = "plug", soporta_wifi = false, soporta_bluetooth = true, palabras_clave_busqueda = "", orden = 99, requiere_vinculacion_bluetooth = false },
            new AparatoTipo { sk_aparato_tipo_id = 18, nombre_tipo = "Ventilador Inteligente", es_asistente = false, icono = "wind", soporta_wifi = true, soporta_bluetooth = true, palabras_clave_busqueda = "FAN,VENTILADOR,VENTILADORES,PWM,VELOCIDAD,DC MOTOR", orden = 4, requiere_vinculacion_bluetooth = false }
        };

        foreach (var deseado in tiposDeseados)
        {
            var existente = db.AparatoTipos.FirstOrDefault(t => t.sk_aparato_tipo_id == deseado.sk_aparato_tipo_id || t.nombre_tipo == deseado.nombre_tipo);
            if (existente == null)
            {
                db.AparatoTipos.Add(deseado);
            }
            else
            {
                existente.nombre_tipo = deseado.nombre_tipo;
                existente.icono = deseado.icono;
                existente.es_asistente = deseado.es_asistente;
                existente.soporta_wifi = deseado.soporta_wifi;
                existente.soporta_bluetooth = deseado.soporta_bluetooth;
                existente.palabras_clave_busqueda = deseado.palabras_clave_busqueda;
                existente.orden = deseado.orden;
                existente.requiere_vinculacion_bluetooth = deseado.requiere_vinculacion_bluetooth;
            }
        }
        db.SaveChanges();

        // Eliminar los tipos de aparatos que no estén en la lista deseada
        var idsDeseados = tiposDeseados.Select(t => t.sk_aparato_tipo_id).ToList();
        var nombresDeseados = tiposDeseados.Select(t => t.nombre_tipo).ToList();
        var tiposAEliminar = db.AparatoTipos
            .Where(t => !idsDeseados.Contains(t.sk_aparato_tipo_id) && !nombresDeseados.Contains(t.nombre_tipo))
            .ToList();

        if (tiposAEliminar.Any())
        {
            try
            {
                db.AparatoTipos.RemoveRange(tiposAEliminar);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "No se pudieron eliminar algunos tipos de aparatos antiguos (posiblemente en uso por llaves foráneas).");
            }
        }
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

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/ws"),
    branch => branch.UseHttpsRedirection());

app.UseStaticFiles();
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

static void EnsureEsp32Schema(PruebaaspContext db)
{
    db.Database.OpenConnection();
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE `gesto_paso` ENGINE=InnoDB;");
        db.Database.ExecuteSqlRaw("ALTER TABLE `gesto` ENGINE=InnoDB;");

        if (!ColumnExists(db, "aparato_configuracion_red", "estado_encendido"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `estado_encendido` tinyint(1) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "fecha_estado_actualizado"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `fecha_estado_actualizado` datetime(6) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "origen_estado"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `origen_estado` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
                """);
        }

        if (!TableExists(db, "aparato_mensaje"))
        {
            db.Database.ExecuteSqlRaw("""
                CREATE TABLE `aparato_mensaje` (
                    `sk_mensaje_id` bigint NOT NULL AUTO_INCREMENT,
                    `sk_aparato_configuracion_red_id` int NOT NULL,
                    `direccion` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
                    `payload_json` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
                    `comando` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
                    `procesado` tinyint(1) NOT NULL,
                    `error_procesamiento` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
                    `fecha_creacion` datetime(6) NOT NULL,
                    PRIMARY KEY (`sk_mensaje_id`),
                    INDEX `IX_aparato_mensaje_sk_aparato_configuracion_red_id` (`sk_aparato_configuracion_red_id`),
                    CONSTRAINT `FK_aparato_mensaje_aparato_configuracion_red`
                        FOREIGN KEY (`sk_aparato_configuracion_red_id`)
                        REFERENCES `aparato_configuracion_red` (`sk_aparato_configuracion_red_id`)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);
        }
        else
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_mensaje` ENGINE=InnoDB;
                """);
        }

        if (!MigrationApplied(db, "20260701120000_AddEsp32EstadoYMensajes"))
        {
            db.Database.ExecuteSqlRaw("""
                INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                VALUES ('20260701120000_AddEsp32EstadoYMensajes', '9.0.16');
                """);
        }
    }
    finally
    {
        db.Database.CloseConnection();
    }
}

static void EnsureConsumoSchema(PruebaaspContext db)
{
    db.Database.OpenConnection();
    try
    {
        if (!ColumnExists(db, "aparato_configuracion_red", "corriente_actual"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `corriente_actual` decimal(8,3) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "potencia_actual"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `potencia_actual` decimal(10,2) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "energia_acumulada_wh"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `energia_acumulada_wh` decimal(12,3) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "fecha_medicion_consumo"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `fecha_medicion_consumo` datetime(6) NULL;
                """);
        }

        if (!TableExists(db, "aparato_consumo_historico"))
        {
            db.Database.ExecuteSqlRaw("""
                CREATE TABLE `aparato_consumo_historico` (
                    `sk_consumo_id` bigint NOT NULL AUTO_INCREMENT,
                    `sk_aparato_configuracion_red_id` int NOT NULL,
                    `corriente_a` decimal(8,3) NOT NULL,
                    `potencia_w` decimal(10,2) NOT NULL,
                    `energia_wh` decimal(12,3) NOT NULL,
                    `fecha_medicion` datetime(6) NOT NULL,
                    PRIMARY KEY (`sk_consumo_id`),
                    INDEX `IX_aparato_consumo_historico_sk_aparato_configuracion_red_id` (`sk_aparato_configuracion_red_id`),
                    INDEX `IX_aparato_consumo_historico_fecha_medicion` (`fecha_medicion`),
                    CONSTRAINT `FK_aparato_consumo_historico_aparato_configuracion_red`
                        FOREIGN KEY (`sk_aparato_configuracion_red_id`)
                        REFERENCES `aparato_configuracion_red` (`sk_aparato_configuracion_red_id`)
                        ON DELETE CASCADE
                ) ENGINE=InnoDB CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                """);
        }
        else
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_consumo_historico` ENGINE=InnoDB;
                """);
        }

        if (!MigrationApplied(db, "20260702183000_AddConsumoHistorico"))
        {
            db.Database.ExecuteSqlRaw("""
                INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                VALUES ('20260702183000_AddConsumoHistorico', '9.0.16');
                """);
        }
    }
    finally
    {
        db.Database.CloseConnection();
    }
}

static void EnsureMultiSocketSchema(PruebaaspContext db)
{
    db.Database.OpenConnection();
    try
    {
        if (!ColumnExists(db, "aparato_configuracion_red", "estado_encendido_2"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `estado_encendido_2` tinyint(1) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "estado_encendido_3"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `estado_encendido_3` tinyint(1) NULL;
                """);
        }

        if (!ColumnExists(db, "aparato_configuracion_red", "estado_encendido_4"))
        {
            db.Database.ExecuteSqlRaw("""
                ALTER TABLE `aparato_configuracion_red`
                ADD COLUMN `estado_encendido_4` tinyint(1) NULL;
                """);
        }
    }
    finally
    {
        db.Database.CloseConnection();
    }
}

static bool ColumnExists(PruebaaspContext db, string tableName, string columnName)
{
    using var command = CreateSchemaCommand(
        db,
        """
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = @tableName
          AND COLUMN_NAME = @columnName;
        """);

    AddParameter(command, "@tableName", tableName);
    AddParameter(command, "@columnName", columnName);
    return Convert.ToInt32(command.ExecuteScalar()) > 0;
}

static bool MigrationApplied(PruebaaspContext db, string migrationId)
{
    using var command = CreateSchemaCommand(
        db,
        """
        SELECT COUNT(*)
        FROM `__EFMigrationsHistory`
        WHERE `MigrationId` = @migrationId;
        """);

    AddParameter(command, "@migrationId", migrationId);
    return Convert.ToInt32(command.ExecuteScalar()) > 0;
}

static bool TableExists(PruebaaspContext db, string tableName)
{
    using var command = CreateSchemaCommand(
        db,
        """
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = @tableName;
        """);

    AddParameter(command, "@tableName", tableName);
    return Convert.ToInt32(command.ExecuteScalar()) > 0;
}

static DbCommand CreateSchemaCommand(PruebaaspContext db, string commandText)
{
    var command = db.Database.GetDbConnection().CreateCommand();
    command.CommandText = commandText;
    return command;
}

static void AddParameter(DbCommand command, string name, object value)
{
    var parameter = command.CreateParameter();
    parameter.ParameterName = name;
    parameter.Value = value;
    command.Parameters.Add(parameter);
}
