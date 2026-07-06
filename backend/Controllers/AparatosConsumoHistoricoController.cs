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
        [HttpGet("{usuarioId}/consumo_historico")]
        public async Task<ActionResult<IEnumerable<AparatoConsumoDto>>> GetConsumoHistorico(
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
    }
}