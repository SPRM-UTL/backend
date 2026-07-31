using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AparatosConsumoHistoricoController : Controller
    {
        public readonly PruebaaspContext _context;

        public AparatosConsumoHistoricoController(PruebaaspContext context)
        {
            _context = context;
        }

        // Historial de todos los aparatos de un usuario
        [HttpGet("usuario/{usuarioId}/consumo_historico")]
        public async Task<ActionResult<IEnumerable<AparatoConsumoDto>>> GetConsumoHistoricoPorUsuario(
            [FromRoute] int usuarioId,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var usuarioSolicitadoId = usuarioId;

            if (usuarioSolicitadoId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            limit = Math.Clamp(limit, 1, 2000);

            var query = from consumo in _context.AparatoConsumoHistoricos
                        join config in _context.AparatoConfiguracionesRed
                            on consumo.sk_aparato_configuracion_red_id equals config.sk_aparato_configuracion_red_id
                        join aparato in _context.Aparatos
                            on config.sk_aparato_id equals aparato.sk_aparato_id
                        where aparato.sk_usuario_id == usuarioSolicitadoId
                        select new { consumo, config.sk_aparato_id };

            if (desde.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion <= hasta.Value);

            var historico = await query
                .OrderBy(q => q.consumo.fecha_medicion)
                .Take(limit)
                .Select(q => new AparatoConsumoDto
                {
                    SkConsumoId = q.consumo.sk_consumo_id,
                    SkAparatoId = q.sk_aparato_id,
                    CorrienteA = q.consumo.corriente_a,
                    PotenciaW = q.consumo.potencia_w,
                    EnergiaWh = q.consumo.energia_wh,
                    FechaMedicion = q.consumo.fecha_medicion
                })
                .ToListAsync();

            if (!historico.Any())
            {
                return NotFound("No se encontraron registros de consumo para tus aparatos.");
            }

            return Ok(historico);
        }

        [HttpPost("registrar")]
        [HttpPost("consumo")]
        public async Task<ActionResult<AparatoConsumoDto>> RegistrarConsumo([FromBody] RegistrarConsumoDto dto)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];
            if (usuarioId == null || usuarioId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            if (dto == null)
            {
                return BadRequest("Datos de consumo inválidos.");
            }

            AparatoConfiguracionRed? config = null;

            if (dto.SkAparatoConfiguracionRedId.HasValue && dto.SkAparatoConfiguracionRedId.Value > 0)
            {
                config = await _context.AparatoConfiguracionesRed
                    .Include(c => c.Aparato)
                    .FirstOrDefaultAsync(c => c.sk_aparato_configuracion_red_id == dto.SkAparatoConfiguracionRedId.Value &&
                                               c.Aparato != null && c.Aparato.sk_usuario_id == usuarioId);
            }
            else if (dto.SkAparatoId.HasValue && dto.SkAparatoId.Value > 0)
            {
                var aparato = await _context.Aparatos
                    .Include(a => a.ConfiguracionRed)
                    .FirstOrDefaultAsync(a => a.sk_aparato_id == dto.SkAparatoId.Value && a.sk_usuario_id == usuarioId);

                config = aparato?.ConfiguracionRed;
            }

            if (config == null)
            {
                return NotFound(new { message = "No se encontró la configuración de red o el aparato no pertenece al usuario." });
            }

            var fechaMedicion = dto.FechaMedicion ?? DateTime.UtcNow;
            decimal energiaWh = dto.EnergiaWh ?? ((config.energia_acumulada_wh ?? 0m) + (dto.PotenciaW > 0 ? dto.PotenciaW / 1000m : 0m));

            config.corriente_actual = dto.CorrienteA;
            config.potencia_actual = dto.PotenciaW;
            config.energia_acumulada_wh = energiaWh;
            config.fecha_medicion_consumo = fechaMedicion;

            var nuevoConsumo = new AparatoConsumoHistorico
            {
                sk_aparato_configuracion_red_id = config.sk_aparato_configuracion_red_id,
                corriente_a = dto.CorrienteA,
                potencia_w = dto.PotenciaW,
                energia_wh = energiaWh,
                fecha_medicion = fechaMedicion
            };

            _context.AparatoConsumoHistoricos.Add(nuevoConsumo);
            await _context.SaveChangesAsync();

            return Ok(new AparatoConsumoDto
            {
                SkConsumoId = nuevoConsumo.sk_consumo_id,
                SkAparatoId = config.sk_aparato_id,
                CorrienteA = nuevoConsumo.corriente_a,
                PotenciaW = nuevoConsumo.potencia_w,
                EnergiaWh = nuevoConsumo.energia_wh,
                FechaMedicion = nuevoConsumo.fecha_medicion
            });
        }

        [HttpPost("registrar_lote")]
        public async Task<ActionResult<IEnumerable<AparatoConsumoDto>>> RegistrarConsumoLote([FromBody] List<RegistrarConsumoDto> dtos)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];
            if (usuarioId == null || usuarioId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            if (dtos == null || !dtos.Any())
            {
                return BadRequest("Lista de consumos vacía o inválida.");
            }

            var creados = new List<AparatoConsumoDto>();

            foreach (var dto in dtos)
            {
                AparatoConfiguracionRed? config = null;

                if (dto.SkAparatoConfiguracionRedId.HasValue && dto.SkAparatoConfiguracionRedId.Value > 0)
                {
                    config = await _context.AparatoConfiguracionesRed
                        .Include(c => c.Aparato)
                        .FirstOrDefaultAsync(c => c.sk_aparato_configuracion_red_id == dto.SkAparatoConfiguracionRedId.Value &&
                                                   c.Aparato != null && c.Aparato.sk_usuario_id == usuarioId);
                }
                else if (dto.SkAparatoId.HasValue && dto.SkAparatoId.Value > 0)
                {
                    var aparato = await _context.Aparatos
                        .Include(a => a.ConfiguracionRed)
                        .FirstOrDefaultAsync(a => a.sk_aparato_id == dto.SkAparatoId.Value && a.sk_usuario_id == usuarioId);

                    config = aparato?.ConfiguracionRed;
                }

                if (config != null)
                {
                    var fechaMedicion = dto.FechaMedicion ?? DateTime.UtcNow;
                    decimal energiaWh = dto.EnergiaWh ?? ((config.energia_acumulada_wh ?? 0m) + (dto.PotenciaW > 0 ? dto.PotenciaW / 1000m : 0m));

                    config.corriente_actual = dto.CorrienteA;
                    config.potencia_actual = dto.PotenciaW;
                    config.energia_acumulada_wh = energiaWh;
                    config.fecha_medicion_consumo = fechaMedicion;

                    var nuevoConsumo = new AparatoConsumoHistorico
                    {
                        sk_aparato_configuracion_red_id = config.sk_aparato_configuracion_red_id,
                        corriente_a = dto.CorrienteA,
                        potencia_w = dto.PotenciaW,
                        energia_wh = energiaWh,
                        fecha_medicion = fechaMedicion
                    };

                    _context.AparatoConsumoHistoricos.Add(nuevoConsumo);
                    creados.Add(new AparatoConsumoDto
                    {
                        SkConsumoId = nuevoConsumo.sk_consumo_id,
                        SkAparatoId = config.sk_aparato_id,
                        CorrienteA = nuevoConsumo.corriente_a,
                        PotenciaW = nuevoConsumo.potencia_w,
                        EnergiaWh = nuevoConsumo.energia_wh,
                        FechaMedicion = nuevoConsumo.fecha_medicion
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(creados);
        }

        [HttpGet("todos_los_consumos/resumen")]
        public async Task<ActionResult<AparatoConsumoResumenDto>> GetConsumoResumenGlobal(
            [FromQuery] string granularidad = "dia",
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];

            var query = from consumo in _context.AparatoConsumoHistoricos
                        join config in _context.AparatoConfiguracionesRed
                            on consumo.sk_aparato_configuracion_red_id equals config.sk_aparato_configuracion_red_id
                        join aparato in _context.Aparatos
                            on config.sk_aparato_id equals aparato.sk_aparato_id
                        where aparato.sk_usuario_id == usuarioId 
                        select new { consumo, config.sk_aparato_configuracion_red_id };

            if (desde.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion <= hasta.Value);

            var lecturasAnom = await query.OrderBy(q => q.consumo.fecha_medicion).ToListAsync();

            var resumen = new AparatoConsumoResumenDto
            {
                Granularidad = granularidad,
                Desde = desde ?? (lecturasAnom.Any() ? lecturasAnom.Min(l => l.consumo.fecha_medicion) : DateTime.UtcNow.Date),
                Hasta = hasta ?? (lecturasAnom.Any() ? lecturasAnom.Max(l => l.consumo.fecha_medicion) : DateTime.UtcNow.Date),
                Puntos = new List<AparatoConsumoPuntoDto>()
            };

            if (!lecturasAnom.Any())
                return Ok(resumen);

            var gran = (granularidad ?? "dia").ToLower().Trim();

            Func<DateTime, DateTime> keySelector = gran switch
            {
                "envivo" => d => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0, d.Kind),
                "hora" or "hoy" => d => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0, d.Kind),
                "ano" or "año" => d => new DateTime(d.Year, d.Month, 1, 0, 0, 0, d.Kind),
                _ => d => d.Date
            };

            var gruposTemporales = lecturasAnom.GroupBy(l => keySelector(l.consumo.fecha_medicion));

            foreach (var grupo in gruposTemporales)
            {
                float energiaTotal = 0;
                float potenciaTotal = 0;
                float corrienteTotal = 0;
                var porDispositivo = grupo.GroupBy(g => g.sk_aparato_configuracion_red_id);
                foreach (var devGrp in porDispositivo)
                {
                    var items = devGrp.ToList();
                    decimal deltaWh = items.Max(l => l.consumo.energia_wh) - items.Min(l => l.consumo.energia_wh);
                    if (deltaWh > 0m)
                    {
                        energiaTotal += (float)deltaWh;
                    }
                    else
                    {
                        decimal sumWh = items.Sum(l => l.consumo.energia_wh);
                        if (sumWh > 0m && items.Count == 1)
                        {
                            energiaTotal += (float)sumWh;
                        }
                        else
                        {
                            double avgW = items.Average(l => (double)l.consumo.potencia_w);
                            double durationHours = (gran == "envivo") ? (1.0 / 60.0) : ((gran == "hora" || gran == "hoy") ? 1.0 : ((gran == "ano" || gran == "año") ? 720.0 : 24.0));
                            energiaTotal += (float)(avgW * durationHours);
                        }
                    }

                    double avgPotencia = items.Average(l => (double)l.consumo.potencia_w);
                    double avgCorriente = items.Average(l => (double)l.consumo.corriente_a);
                    potenciaTotal += (float)avgPotencia;
                    corrienteTotal += (float)avgCorriente;
                }

                var p = new AparatoConsumoPuntoDto
                {
                    Periodo = grupo.Key,
                    PotenciaPromedioW = potenciaTotal,
                    CorrientePromedioA = corrienteTotal,
                    EnergiaConsumidaWh = energiaTotal
                };
                resumen.Puntos.Add(p);
            }

            return Ok(resumen);
        }

        // Mantienes tu endpoint individual original abajo si lo necesitas...
        [HttpGet("aparato/{sk_aparato_id}/consumo_historico")]
        public async Task<ActionResult<IEnumerable<AparatoConsumoDto>>> GetConsumoHistorico(
            int sk_aparato_id,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];
            var aparato = await _context.Aparatos
                .Include(a => a.ConfiguracionRed)
                .FirstOrDefaultAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId);

            if (aparato?.ConfiguracionRed == null)
            {
                return NotFound("El aparato no tiene configuracion de red o no te pertenece");
            }

            limit = Math.Clamp(limit, 1, 500);
            var query = _context.AparatoConsumoHistoricos
                .Where(c => c.sk_aparato_configuracion_red_id == aparato.ConfiguracionRed.sk_aparato_configuracion_red_id);

            if (desde.HasValue) query = query.Where(c => c.fecha_medicion >= desde.Value);
            if (hasta.HasValue) query = query.Where(c => c.fecha_medicion <= hasta.Value);

            var historico = await query
                .OrderBy(c => c.fecha_medicion)
                .Take(limit)
                .Select(c => new AparatoConsumoDto
                {
                    SkConsumoId = c.sk_consumo_id,
                    SkAparatoId = sk_aparato_id,
                    CorrienteA = c.corriente_a,
                    PotenciaW = c.potencia_w,
                    EnergiaWh = c.energia_wh,
                    FechaMedicion = c.fecha_medicion
                }).ToListAsync();


            return Ok(historico);
        }

        [HttpGet("usuario/{usuarioId}/resumen_dona")]
        [HttpGet("{usuarioId}/resumen_dona")]
        public async Task<ActionResult<IEnumerable<object>>> GetConsumoDonaPorUsuario(
            [FromRoute] int usuarioId,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var tokenUserId = (int?)HttpContext.Items["UsuarioId"];
            var targetUserId = (usuarioId > 0) ? usuarioId : (tokenUserId ?? 0);

            if (targetUserId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var query = from consumo in _context.AparatoConsumoHistoricos
                        join config in _context.AparatoConfiguracionesRed
                            on consumo.sk_aparato_configuracion_red_id equals config.sk_aparato_configuracion_red_id
                        join aparato in _context.Aparatos
                            on config.sk_aparato_id equals aparato.sk_aparato_id
                        where aparato.sk_usuario_id == targetUserId
                        select new { consumo, aparato.nombre_aparato };

            if (desde.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion <= hasta.Value);

            var lecturas = await query.ToListAsync();

            // Suma de deltas diarios por dispositivo (igual que el gráfico histórico)
            // Esto soporta correctamente contadores que se reinician cada mes.
            var resultado = lecturas
                .GroupBy(q => q.nombre_aparato)
                .Select(g => {
                    var items = g.OrderBy(x => x.consumo.fecha_medicion).ToList();

                    // Agrupamos por día y calculamos el delta de cada día
                    decimal totalEnergiaWh = items
                        .GroupBy(x => x.consumo.fecha_medicion.Date)
                        .Sum(dia => {
                            var diasItems = dia.ToList();
                            decimal diaMax = diasItems.Max(x => x.consumo.energia_wh);
                            decimal diaMin = diasItems.Min(x => x.consumo.energia_wh);
                            decimal diaDelta = diaMax - diaMin;
                            if (diaDelta > 0m) return diaDelta;

                            // Si no hay delta (un solo registro en el día), usamos potencia × 1h
                            double avgW = diasItems.Average(x => (double)x.consumo.potencia_w);
                            return (decimal)(avgW * 1.0);
                        });

                    return new
                    {
                        Aparato = g.Key,
                        TotalEnergiaWh = totalEnergiaWh
                    };
                })
                .ToList();

            return Ok(resultado);
        }
    }
}