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

        public Task<bool?> UpdatePowerStateAsync(
            int configuracionRedId,
            bool encendido,
            string origen,
            CancellationToken cancellationToken = default)
        {
            return UpdatePowerStateAsync(configuracionRedId, encendido, origen, 1, cancellationToken);
        }

        public async Task<bool?> UpdatePowerStateAsync(
            int configuracionRedId,
            bool encendido,
            string origen,
            int contacto,
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

            ApplyOutletState(config, contacto, encendido, DateTime.UtcNow, origen);
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

        public bool TryParseOutletCommand(string message, out int contacto, out bool encendido)
        {
            contacto = 0;
            encendido = false;

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var trimmed = message.Trim();
            if (TryParseOutletTextCommand(trimmed, out contacto, out encendido))
            {
                return true;
            }

            if (!trimmed.StartsWith('{'))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                if (root.TryGetProperty("comando", out var comandoProp) &&
                    comandoProp.ValueKind == JsonValueKind.String &&
                    TryParseOutletTextCommand(comandoProp.GetString(), out contacto, out encendido))
                {
                    return true;
                }

                if (root.TryGetProperty("command", out var commandProp) &&
                    commandProp.ValueKind == JsonValueKind.String &&
                    TryParseOutletTextCommand(commandProp.GetString(), out contacto, out encendido))
                {
                    return true;
                }

                var contactoJson = ReadInt(root, "contacto", "outlet", "relay", "rele");
                var estadoJson = ReadBoolean(root, "encendido", "estado", "value");
                if (contactoJson is >= 1 and <= 4 && estadoJson.HasValue)
                {
                    contacto = contactoJson.Value;
                    encendido = estadoJson.Value;
                    return true;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogDebug(ex, "Mensaje ESP32 no es JSON de comando MultiSocket válido.");
            }

            return false;
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
            if (TryParseOutletCommand(message, out var contacto, out var outletState))
            {
                var origen = message.TrimStart().StartsWith('{') ? "esp32_json" : "esp32_ack";
                await UpdatePowerStateAsync(configuracionRedId, outletState, origen, contacto, cancellationToken);
            }
            else if (powerState.HasValue)
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
            var hasOutletCommand = TryParseOutletCommand(comando, out var contacto, out var outletState);
            var payload = new
            {
                @event = "command",
                comando,
                estado = hasOutletCommand ? outletState : estadoEncendido,
                contacto = hasOutletCommand ? contacto : (int?)null,
                origen,
                fecha = DateTime.UtcNow
            };

            await LogMessageAsync(
                configuracionRedId,
                "outbound",
                payload,
                comando: comando,
                cancellationToken: cancellationToken);

            if (hasOutletCommand)
            {
                await UpdatePowerStateAsync(configuracionRedId, outletState, origen, contacto, cancellationToken);
            }
            else if (estadoEncendido.HasValue)
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
                ApplyOutletState(config, 1, telemetry.EstadoEncendido.Value, fechaMedicion, "esp32_telemetry");
            }

            if (telemetry.EstadoEncendido2.HasValue)
            {
                ApplyOutletState(config, 2, telemetry.EstadoEncendido2.Value, fechaMedicion, "esp32_telemetry");
            }

            if (telemetry.EstadoEncendido3.HasValue)
            {
                ApplyOutletState(config, 3, telemetry.EstadoEncendido3.Value, fechaMedicion, "esp32_telemetry");
            }

            if (telemetry.EstadoEncendido4.HasValue)
            {
                ApplyOutletState(config, 4, telemetry.EstadoEncendido4.Value, fechaMedicion, "esp32_telemetry");
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
                    EstadoEncendido = ReadBoolean(root, "estado1", "encendido1", "rele1") ?? TryParsePowerState(message),
                    EstadoEncendido2 = ReadBoolean(root, "estado2", "encendido2", "rele2"),
                    EstadoEncendido3 = ReadBoolean(root, "estado3", "encendido3", "rele3"),
                    EstadoEncendido4 = ReadBoolean(root, "estado4", "encendido4", "rele4")
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

        private static bool? ReadBoolean(JsonElement root, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (root.TryGetProperty(propertyName, out var value))
                {
                    var parsed = ReadBoolean(value);
                    if (parsed.HasValue)
                    {
                        return parsed;
                    }
                }
            }

            return null;
        }

        private static bool? ReadBoolean(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number when value.TryGetInt32(out var number) => number != 0,
                JsonValueKind.String => ParseBooleanText(value.GetString()),
                _ => null
            };
        }

        private static bool? ParseBooleanText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Equals("ON", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (normalized.Equals("OFF", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return null;
        }

        private static int? ReadInt(JsonElement root, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!root.TryGetProperty(propertyName, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        public sealed class TelemetryReading
        {
            public decimal CorrienteA { get; init; }
            public decimal PotenciaW { get; init; }
            public decimal EnergiaWh { get; init; }
            public bool? EstadoEncendido { get; init; }
            public bool? EstadoEncendido2 { get; init; }
            public bool? EstadoEncendido3 { get; init; }
            public bool? EstadoEncendido4 { get; init; }
        }

        private static void ApplyOutletState(
            AparatoConfiguracionRed config,
            int contacto,
            bool encendido,
            DateTime fecha,
            string origen)
        {
            switch (contacto)
            {
                case 1:
                    config.estado_encendido = encendido;
                    break;
                case 2:
                    config.estado_encendido_2 = encendido;
                    break;
                case 3:
                    config.estado_encendido_3 = encendido;
                    break;
                case 4:
                    config.estado_encendido_4 = encendido;
                    break;
                default:
                    return;
            }

            config.fecha_estado_actualizado = fecha;
            config.origen_estado = origen;
        }

        private static bool TryParseOutletTextCommand(string? command, out int contacto, out bool encendido)
        {
            contacto = 0;
            encendido = false;

            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            var normalized = command.Trim()
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .ToUpperInvariant();

            if (normalized.Length == 3 &&
                normalized.StartsWith("ON", StringComparison.Ordinal) &&
                int.TryParse(normalized[2].ToString(), out contacto) &&
                contacto is >= 1 and <= 4)
            {
                encendido = true;
                return true;
            }

            if (normalized.Length == 4 &&
                normalized.StartsWith("OFF", StringComparison.Ordinal) &&
                int.TryParse(normalized[3].ToString(), out contacto) &&
                contacto is >= 1 and <= 4)
            {
                encendido = false;
                return true;
            }

            return false;
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
