using System.Text.Json;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class Esp32DeviceStateService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Esp32DeviceStateService> _logger;

        public Esp32DeviceStateService(
            IServiceScopeFactory scopeFactory,
            ILogger<Esp32DeviceStateService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task LogMessageAsync(
            int configuracionRedId,
            string direccion,
            object payload,
            string? comando = null,
            bool procesado = true,
            string? error = null,
            CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            var mensaje = new AparatoMensaje
            {
                sk_aparato_configuracion_red_id = configuracionRedId,
                direccion = direccion,
                payload_json = JsonSerializer.Serialize(payload),
                comando = comando,
                procesado = procesado,
                error_procesamiento = error,
                fecha_creacion = DateTime.UtcNow
            };

            db.AparatoMensajes.Add(mensaje);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool?> UpdatePowerStateAsync(
            int configuracionRedId,
            bool encendido,
            string origen,
            CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            var config = await db.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.sk_aparato_configuracion_red_id == configuracionRedId, cancellationToken);

            if (config is null)
            {
                return null;
            }

            config.estado_encendido = encendido;
            config.fecha_estado_actualizado = DateTime.UtcNow;
            config.origen_estado = origen;
            await db.SaveChangesAsync(cancellationToken);
            return encendido;
        }

        public bool? TryParsePowerState(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            var trimmed = message.Trim();

            if (trimmed.Equals("ON", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (trimmed.Equals("OFF", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!trimmed.StartsWith('{'))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                if (root.TryGetProperty("encendido", out var encendidoProp) &&
                    encendidoProp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    return encendidoProp.GetBoolean();
                }

                if (root.TryGetProperty("estado", out var estadoProp))
                {
                    if (estadoProp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    {
                        return estadoProp.GetBoolean();
                    }

                    if (estadoProp.ValueKind == JsonValueKind.String)
                    {
                        var estadoTexto = estadoProp.GetString();
                        if (estadoTexto?.Equals("ON", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            return true;
                        }

                        if (estadoTexto?.Equals("OFF", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            return false;
                        }
                    }
                }

                if (root.TryGetProperty("event", out var eventProp) &&
                    eventProp.GetString()?.Equals("state", StringComparison.OrdinalIgnoreCase) == true &&
                    root.TryGetProperty("value", out var valueProp) &&
                    valueProp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    return valueProp.GetBoolean();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Mensaje ESP32 no es JSON de estado válido.");
            }

            return null;
        }

        public async Task ProcessInboundMessageAsync(
            int configuracionRedId,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (TryParseTelemetry(message, out var telemetry))
            {
                await ProcessTelemetryAsync(configuracionRedId, telemetry, cancellationToken);
                return;
            }

            var payload = TryDeserializeJson(message, out var jsonRoot)
                ? jsonRoot!
                : new { raw = message };

            await LogMessageAsync(
                configuracionRedId,
                "inbound",
                payload,
                comando: ExtractComando(message),
                cancellationToken: cancellationToken);

            var powerState = TryParsePowerState(message);
            if (powerState.HasValue)
            {
                var origen = message.TrimStart().StartsWith('{') ? "esp32_json" : "esp32_ack";
                await UpdatePowerStateAsync(configuracionRedId, powerState.Value, origen, cancellationToken);
            }
        }

        public async Task ProcessOutboundCommandAsync(
            int configuracionRedId,
            string comando,
            bool? estadoEncendido,
            string origen,
            CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                @event = "command",
                comando,
                estado = estadoEncendido,
                origen,
                fecha = DateTime.UtcNow
            };

            await LogMessageAsync(
                configuracionRedId,
                "outbound",
                payload,
                comando: comando,
                cancellationToken: cancellationToken);

            if (estadoEncendido.HasValue)
            {
                await UpdatePowerStateAsync(configuracionRedId, estadoEncendido.Value, origen, cancellationToken);
            }
        }

        public async Task ProcessTelemetryAsync(
            int configuracionRedId,
            TelemetryReading telemetry,
            CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            var config = await db.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.sk_aparato_configuracion_red_id == configuracionRedId, cancellationToken);

            if (config is null)
            {
                return;
            }

            var fechaMedicion = DateTime.UtcNow;
            config.corriente_actual = telemetry.CorrienteA;
            config.potencia_actual = telemetry.PotenciaW;
            config.energia_acumulada_wh = telemetry.EnergiaWh;
            config.fecha_medicion_consumo = fechaMedicion;

            if (telemetry.EstadoEncendido.HasValue)
            {
                config.estado_encendido = telemetry.EstadoEncendido;
                config.fecha_estado_actualizado = fechaMedicion;
                config.origen_estado = "esp32_telemetry";
            }

            db.AparatoConsumoHistoricos.Add(new AparatoConsumoHistorico
            {
                sk_aparato_configuracion_red_id = configuracionRedId,
                corriente_a = telemetry.CorrienteA,
                potencia_w = telemetry.PotenciaW,
                energia_wh = telemetry.EnergiaWh,
                fecha_medicion = fechaMedicion
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        public bool TryParseTelemetry(string message, out TelemetryReading telemetry)
        {
            telemetry = default!;
            if (string.IsNullOrWhiteSpace(message) || !message.TrimStart().StartsWith('{'))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                var hasTelemetryEvent = root.TryGetProperty("event", out var eventProp) &&
                    eventProp.GetString()?.Equals("telemetry", StringComparison.OrdinalIgnoreCase) == true;

                var hasMeterFields =
                    root.TryGetProperty("corriente", out _) ||
                    root.TryGetProperty("potencia", out _) ||
                    root.TryGetProperty("energia", out _);

                if (!hasTelemetryEvent)
                {
                    return false;
                }

                if (hasTelemetryEvent && !hasMeterFields)
                {
                    return false;
                }

                telemetry = new TelemetryReading
                {
                    CorrienteA = ReadDecimal(root, "corriente"),
                    PotenciaW = ReadDecimal(root, "potencia"),
                    EnergiaWh = ReadDecimal(root, "energia"),
                    EstadoEncendido = TryParsePowerState(message)
                };

                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Mensaje ESP32 no es telemetría válida.");
                return false;
            }
        }

        private static decimal ReadDecimal(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value))
            {
                return 0m;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(value.GetString(), out var parsed) => parsed,
                _ => 0m
            };
        }

        public sealed class TelemetryReading
        {
            public decimal CorrienteA { get; init; }
            public decimal PotenciaW { get; init; }
            public decimal EnergiaWh { get; init; }
            public bool? EstadoEncendido { get; init; }
        }

        private static string? ExtractComando(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            if (!message.TrimStart().StartsWith('{'))
            {
                return message.Trim();
            }

            try
            {
                using var doc = JsonDocument.Parse(message);
                if (doc.RootElement.TryGetProperty("comando", out var comandoProp))
                {
                    return comandoProp.GetString();
                }

                if (doc.RootElement.TryGetProperty("event", out var eventProp))
                {
                    return eventProp.GetString();
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }

        private static bool TryDeserializeJson(string message, out object? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(message) || !message.TrimStart().StartsWith('{'))
            {
                return false;
            }

            try
            {
                result = JsonSerializer.Deserialize<object>(message);
                return result is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
