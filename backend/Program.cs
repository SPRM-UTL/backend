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
        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Asistente"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Asistente", icono = "ic_input_add", es_asistente = true, soporta_wifi = true, soporta_bluetooth = false, orden = 1, palabras_clave_busqueda = null });
        
        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Focos"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Focos", icono = "lightbulb", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, orden = 2, palabras_clave_busqueda = "FOCO,LIGHT,BULB,LAMP" });

        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Enchufe"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Enchufe", icono = "plug", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, requiere_vinculacion_bluetooth = false, orden = 3, palabras_clave_busqueda = "SOCKET,PLUG,ENCHUFE,SMARTPLUG" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Sockets Inteligentes"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Sockets Inteligentes", icono = "plug", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, orden = 4, palabras_clave_busqueda = "SOCKET,PLUG,SMARTPLUG" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "MultiSocket"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "MultiSocket", icono = "plug", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, requiere_vinculacion_bluetooth = false, orden = 5, palabras_clave_busqueda = "MULTISOCKET,MULTI SOCKET,REGLETA,POWERSTRIP,POWER STRIP,MULTIENCHUFE,CONTACTO,CONTACTOS,SOCKET,PLUG" });

        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Ventilador"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Ventilador", icono = "wind", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, requiere_vinculacion_bluetooth = false, orden = 6, palabras_clave_busqueda = "FAN,VENTILADOR" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Ventilador Inteligente"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Ventilador Inteligente", icono = "wind", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, requiere_vinculacion_bluetooth = false, orden = 6, palabras_clave_busqueda = "FAN,VENTILADOR,VENTILADORES,PWM,VELOCIDAD,DC MOTOR" });

        if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Cámara"))
            db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Cámara", icono = "videocam", es_asistente = false, soporta_wifi = true, soporta_bluetooth = true, requiere_vinculacion_bluetooth = false, orden = 7, palabras_clave_busqueda = "CAM,CAMERA,CAMARA,WEBCAM,VIDEO" });


        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Televisión"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Televisión", icono = "tv_minimal", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, orden = 6, palabras_clave_busqueda = "TV,TELEVISION,SCREEN,DISPLAY" });

        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Bocinas"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Bocinas", icono = "speaker", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, orden = 7, palabras_clave_busqueda = "SPEAKER,BOCINA,ALTAVOZ,SOUNDBAR,CHARGE,FLIP,XTREME,BOOMBOX,SOUNDLINK" });

        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Audífonos"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Audífonos", icono = "headphones", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, orden = 8, palabras_clave_busqueda = "HEADPHONE,HEADSET,EARPHONE,EARBUDS,AUDIFONOS,AUDÍFONOS,AURICULAR,AUT,TWS,BUDS,AIRPOD,WH-,WF-,QC,TUNE,FREEBUDS" });

        // if (!db.AparatoTipos.Any(t => t.nombre_tipo == "Luces"))
        //     db.AparatoTipos.Add(new AparatoTipo { nombre_tipo = "Luces", icono = "lamp_floor", es_asistente = false, soporta_wifi = true, soporta_bluetooth = false, orden = 9, palabras_clave_busqueda = "STRIP,TIRA,LUCES,LED,LAMP" });

        db.SaveChanges(); // Guardar nuevas adiciones antes de buscar para actualizar

        // Actualizar iconos y métodos de configuración de aparatos existentes si no tienen
        var tipos = db.AparatoTipos.ToList();
        foreach (var tipo in tipos) {
            var usaWifiBluetooth =
                tipo.nombre_tipo == "Sockets Inteligentes" ||
                tipo.nombre_tipo == "MultiSocket" ||
                tipo.nombre_tipo == "Ventilador Inteligente" ||
                tipo.nombre_tipo == "Cámara";

            // Forzar configuración WiFi-only por defecto a los antiguos
            if (!usaWifiBluetooth) {
                tipo.soporta_bluetooth = false;
                tipo.soporta_wifi = true;
            } else {
                tipo.soporta_bluetooth = true;
                tipo.soporta_wifi = true;
                
                if (tipo.nombre_tipo == "MultiSocket" ||
                    tipo.nombre_tipo == "Ventilador Inteligente" ||
                    tipo.nombre_tipo == "Cámara") {
                    tipo.requiere_vinculacion_bluetooth = false;
                }
            }

            if (string.IsNullOrEmpty(tipo.icono) || tipo.orden == 99 || tipo.orden == 0) {
                switch (tipo.nombre_tipo) {
                    // case "Asistente": tipo.icono = "ic_input_add"; tipo.orden = 1; break;
                    // case "Focos": tipo.icono = "lightbulb"; tipo.orden = 2; break;
                    // case "Enchufe": tipo.icono = "plug"; tipo.orden = 3; break;
                    case "Sockets Inteligentes": tipo.icono = "plug"; tipo.orden = 4; break;
                    case "MultiSocket": tipo.icono = "plug"; tipo.orden = 5; break;
                    case "Ventilador Inteligente": tipo.icono = "wind"; tipo.orden = 6; break;
                    case "Cámara": tipo.icono = "videocam"; tipo.orden = 7; break;
                    // case "Televisión": tipo.icono = "tv_minimal"; tipo.orden = 6; break;
                    // case "Bocinas": tipo.icono = "speaker"; tipo.orden = 7; break;
                    // case "Audífonos": tipo.icono = "headphones"; tipo.orden = 8; break;
                    // case "Luces": tipo.icono = "lamp_floor"; tipo.orden = 9; break;
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
