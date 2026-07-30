using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/Fact_Historico_Actividad")]
    [Route("api/historial-actividad")]
    [ApiController]
    public class HistorialActividadController : ControllerBase
    {
        private readonly PruebaaspContext _context;

        public HistorialActividadController(PruebaaspContext context)
        {
            _context = context;
        }

        // GET: api/Fact_Historico_Actividad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActividadHistorialDto>>> GetFact_Historico_Actividad()
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];

            // PASO 1: Traemos los datos crudos desde la Base de Datos de forma asíncrona
            var datosCrudos = await _context.HistorialActividades
                .Include(h => h.Tiempo)
                .Include(h => h.Usuario)
                .Include(h => h.Aparato)
                    .ThenInclude(a => a.Tipo)
                .Include(h => h.Gesto)
                .Where(h => h.sk_usuario_id == usuarioId)
                .OrderByDescending(h => h.sk_actividad_id)
                .ToListAsync(); // Resolvemos la consulta SQL aquí de forma limpia

            // PASO 2: Transformamos los datos en memoria (C# puro ya sabe procesar Linq sin romper MySQL)
            var historial = datosCrudos.Select(h => new ActividadHistorialDto
            {
                Id = h.sk_actividad_id,

                Hora = h.Tiempo != null 
                    ? $"{h.Tiempo.fecha_completa:dd/MM/yyyy} {h.Tiempo.hora_periodo:00}:00" 
                    : "Desconocida",

                Accion = h.sk_gesto_id == 1 
                    ? $"Activado manualmente por {(h.Usuario != null ? h.Usuario.nombre_usuario : "Usuario")}" 
                    : $"Activado por gesto '{(h.Gesto != null ? h.Gesto.nombre_gesto : "Desconocido")}' ({(h.Usuario != null ? h.Usuario.nombre_usuario : "Usuario")})",

                Dispositivo = h.Aparato != null ? h.Aparato.nombre_aparato ?? "Dispositivo" : "Dispositivo",

                Icono = h.Aparato != null ? (!string.IsNullOrEmpty(h.Aparato.icono) ? h.Aparato.icono : (h.Aparato.Tipo != null ? h.Aparato.Tipo.nombre_tipo : "circle-plus")) : "circle-plus",

                Color = h.Aparato != null && (h.Aparato.nombre_aparato ?? "").ToLower().Contains("luz") ? "#f97316" :
                        h.Aparato != null && (h.Aparato.nombre_aparato ?? "").ToLower().Contains("tv") ? "#8b5cf6" :
                        h.Aparato != null && (h.Aparato.nombre_aparato ?? "").ToLower().Contains("bocina") ? "#ec4899" : "#3b82f6",

                Estado = h.ejecucion_exitosa == true ? "Ejecutado" : "Error",

                Metodo = "Gesto"
            }).ToList();

            return Ok(historial);
        }

        // GET: api/Fact_Historico_Actividad/5
        [HttpGet("{sk_actividad_id}")]
        public async Task<ActionResult<HistorialActividad>> GetFact_Historico_Actividad(int sk_actividad_id)
        {
            var fact_historico_actividad = await _context.HistorialActividades.FindAsync(sk_actividad_id);

            if (fact_historico_actividad == null)
            {
                return NotFound();
            }

            return fact_historico_actividad;
        }

        // PUT: api/Fact_Historico_Actividad/5
        [HttpPut("{sk_actividad_id}")]
        public async Task<IActionResult> PutFact_Historico_Actividad(int? sk_actividad_id, HistorialActividad fact_historico_actividad)
        {
            if (sk_actividad_id != fact_historico_actividad.sk_actividad_id)
            {
                return BadRequest();
            }

            _context.Entry(fact_historico_actividad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Fact_Historico_ActividadExists(sk_actividad_id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Fact_Historico_Actividad
        [HttpPost]
        public async Task<ActionResult<HistorialActividad>> PostFact_Historico_Actividad(HistorialActividad fact_historico_actividad)
        {
            _context.HistorialActividades.Add(fact_historico_actividad);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFact_Historico_Actividad", new { sk_actividad_id = fact_historico_actividad.sk_actividad_id }, fact_historico_actividad);
        }

        // DELETE: api/Fact_Historico_Actividad/5
        [HttpDelete("{sk_actividad_id}")]
        public async Task<IActionResult> DeleteFact_Historico_Actividad(int? sk_actividad_id)
        {
            var fact_historico_actividad = await _context.HistorialActividades.FindAsync(sk_actividad_id);
            if (fact_historico_actividad == null)
            {
                return NotFound();
            }

            _context.HistorialActividades.Remove(fact_historico_actividad);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool Fact_Historico_ActividadExists(int? sk_actividad_id)
        {
            return _context.HistorialActividades.Any(e => e.sk_actividad_id == sk_actividad_id);
        }
    }
}
