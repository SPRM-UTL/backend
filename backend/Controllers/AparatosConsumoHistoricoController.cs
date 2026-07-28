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

            if (granularidad.ToLower() == "envivo")
            {
                var gruposEnVivo = lecturasAnom
                    .GroupBy(l => new DateTime(l.consumo.fecha_medicion.Year, l.consumo.fecha_medicion.Month, l.consumo.fecha_medicion.Day, l.consumo.fecha_medicion.Hour, l.consumo.fecha_medicion.Minute, 0, l.consumo.fecha_medicion.Kind))
                    .OrderByDescending(g => g.Key)
                    .Take(60)
                    .OrderBy(g => g.Key);

                foreach (var grupo in gruposEnVivo)
                {
                    float energiaTotal = 0;
                    var porDispositivo = grupo.GroupBy(g => g.sk_aparato_configuracion_red_id);
                    foreach (var devGrp in porDispositivo)
                    {
                        energiaTotal += (float)(devGrp.Max(l => l.consumo.energia_wh) - devGrp.Min(l => l.consumo.energia_wh));
                    }

                    resumen.Puntos.Add(new AparatoConsumoPuntoDto
                    {
                        Periodo = grupo.Key,
                        PotenciaPromedioW = porDispositivo.Sum(d => (float)d.Average(l => l.consumo.potencia_w)),
                        CorrientePromedioA = porDispositivo.Sum(d => (float)d.Average(l => l.consumo.corriente_a)),
                        EnergiaConsumidaWh = energiaTotal
                    });
                }
                return Ok(resumen);
            }

            var gruposTemporales = granularidad.ToLower() == "año"
                ? lecturasAnom.GroupBy(l => new DateTime(l.consumo.fecha_medicion.Year, l.consumo.fecha_medicion.Month, 1, 0, 0, 0, l.consumo.fecha_medicion.Kind))
                : (granularidad.ToLower() == "mes"
                    ? lecturasAnom.GroupBy(l => l.consumo.fecha_medicion.Date)
                    : lecturasAnom.GroupBy(l => new DateTime(l.consumo.fecha_medicion.Year, l.consumo.fecha_medicion.Month, l.consumo.fecha_medicion.Day, l.consumo.fecha_medicion.Hour, 0, 0, l.consumo.fecha_medicion.Kind)));

            foreach (var grupo in gruposTemporales)
            {
                float energiaTotal = 0;
                var porDispositivo = grupo.GroupBy(g => g.sk_aparato_configuracion_red_id);
                foreach (var devGrp in porDispositivo)
                {
                    energiaTotal += (float)(devGrp.Max(l => l.consumo.energia_wh) - devGrp.Min(l => l.consumo.energia_wh));
                }

                var p = new AparatoConsumoPuntoDto
                {
                    Periodo = grupo.Key,
                    PotenciaPromedioW = porDispositivo.Sum(d => (float)d.Average(l => l.consumo.potencia_w)),
                    CorrientePromedioA = porDispositivo.Sum(d => (float)d.Average(l => l.consumo.corriente_a)),
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
                .FirstOrDefaultAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId); // Corregido aquí también

            if (aparato?.ConfiguracionRed == null)
            {
                return NotFound("El aparato no tiene configuracion de red o no te pertenece");
            }

            limit = Math.Clamp(limit, 1, 500);
            var query = _context.AparatoConsumoHistoricos
                .Where(c => c.sk_aparato_configuracion_red_id == aparato.ConfiguracionRed.sk_aparato_configuracion_red_id);

            if (desde.HasValue) query = query.Where(c => c.fecha_medicion >= desde.Value);
            if (hasta.HasValue) query = query.Where(c => c.fecha_medicion <= hasta.Value); // Corregido aquí también

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

        [HttpGet("{usuarioId}/resumen_dona")]
        public async Task<ActionResult<IEnumerable<object>>> GetConsumoDonaPorUsuario(
        [FromRoute] int usuarioId,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
        {
            if (usuarioId == 0)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var query = from consumo in _context.AparatoConsumoHistoricos
                        join config in _context.AparatoConfiguracionesRed
                            on consumo.sk_aparato_configuracion_red_id equals config.sk_aparato_configuracion_red_id
                        join aparato in _context.Aparatos
                            on config.sk_aparato_id equals aparato.sk_aparato_id
                        where aparato.sk_usuario_id == usuarioId
                        select new { consumo, aparato.nombre_aparato };

            if (desde.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion <= hasta.Value);

            var lecturas = await query.ToListAsync();

            var resultado = lecturas
                .GroupBy(q => q.nombre_aparato)
                .Select(g => new
                {
                    Aparato = g.Key,
                    TotalEnergiaWh = g.Max(x => x.consumo.energia_wh) - g.Min(x => x.consumo.energia_wh)
                })
                .ToList();

            return Ok(resultado);
        }


    }
}