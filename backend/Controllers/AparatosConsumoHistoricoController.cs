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

        // GET: api/AparatosConsumoHistorico/todos_los_consumos
        [HttpGet("todos_los_consumos")]
        public async Task<ActionResult<IEnumerable<AparatoConsumoDto>>> GetConsumoHistoricoTodos(
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];

            limit = Math.Clamp(limit, 1, 2000);

            var query = from consumo in _context.AparatoConsumoHistoricos
                        join config in _context.AparatoConfiguracionesRed
                            on consumo.sk_aparato_configuracion_red_id equals config.sk_aparato_configuracion_red_id
                        join aparato in _context.Aparatos
                            on config.sk_aparato_id equals aparato.sk_aparato_id
                        where aparato.sk_usuario_id == usuarioId 
                        select new { consumo, config.sk_aparato_id };

            if (desde.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(q => q.consumo.fecha_medicion <= hasta.Value);

            var resultado = await query
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

            if (!resultado.Any())
            {
                return NotFound("No se encontraron registros de consumo para tus aparatos.");
            }

            return Ok(resultado);
        }

        // Mantienes tu endpoint individual original abajo si lo necesitas...
        [HttpGet("{sk_aparato_id}/consumo_historico")]
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
    }
}