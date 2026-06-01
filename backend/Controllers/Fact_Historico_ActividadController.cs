using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Fact_Historico_ActividadController : ControllerBase
    {
        private readonly PruebaaspContext _context;

        public Fact_Historico_ActividadController(PruebaaspContext context)
        {
            _context = context;
        }

        // GET: api/Fact_Historico_Actividad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActividadHistorialDto>>> GetFact_Historico_Actividad()
        {
            // PASO 1: Traemos los datos crudos desde la Base de Datos de forma asíncrona
            var datosCrudos = await _context.Historico_Actividad
                .Include(h => h.Dim_Tiempo)
                .Include(h => h.Dim_Usuario)
                .Include(h => h.Dim_Aparato)
                .OrderByDescending(h => h.sk_actividad_id)
                .ToListAsync(); // Resolvemos la consulta SQL aquí de forma limpia

            // PASO 2: Transformamos los datos en memoria (C# puro ya sabe procesar Linq sin romper MySQL)
            var historial = datosCrudos.Select(h => new ActividadHistorialDto
            {
                Id = h.sk_actividad_id,

                // Ahora sí podemos usar .ToString() de forma segura porque está en memoria
                Hora = h.Dim_Tiempo != null ? h.Dim_Tiempo.hora_periodo.ToString() : "0",

                Accion = $"Gesto detectado por {(h.Dim_Usuario != null ? h.Dim_Usuario.nombre_usuario : "Usuario")}",

                Dispositivo = h.Dim_Aparato != null ? h.Dim_Aparato.nombre_aparato ?? "Dispositivo" : "Dispositivo",

                Icono = h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("luz") ? "lightbulb" :
                        h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("tv") ? "tv" :
                        h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("bocina") ? "speaker" :
                        h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("vent") ? "fan" : "air-vent",

                Color = h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("luz") ? "#f97316" :
                        h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("tv") ? "#8b5cf6" :
                        h.Dim_Aparato != null && (h.Dim_Aparato.nombre_aparato ?? "").ToLower().Contains("bocina") ? "#ec4899" : "#3b82f6",

                Estado = h.ejecucion_exitosa == true ? "Ejecutado" : "Error",

                Metodo = "Gesto"
            }).ToList();

            return Ok(historial);
        }

        // GET: api/Fact_Historico_Actividad/5
        [HttpGet("{sk_actividad_id}")]
        public async Task<ActionResult<Fact_Historico_Actividad>> GetFact_Historico_Actividad(int sk_actividad_id)
        {
            var fact_historico_actividad = await _context.Historico_Actividad.FindAsync(sk_actividad_id);

            if (fact_historico_actividad == null)
            {
                return NotFound();
            }

            return fact_historico_actividad;
        }

        // PUT: api/Fact_Historico_Actividad/5
        [HttpPut("{sk_actividad_id}")]
        public async Task<IActionResult> PutFact_Historico_Actividad(int? sk_actividad_id, Fact_Historico_Actividad fact_historico_actividad)
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
        public async Task<ActionResult<Fact_Historico_Actividad>> PostFact_Historico_Actividad(Fact_Historico_Actividad fact_historico_actividad)
        {
            _context.Historico_Actividad.Add(fact_historico_actividad);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFact_Historico_Actividad", new { sk_actividad_id = fact_historico_actividad.sk_actividad_id }, fact_historico_actividad);
        }

        // DELETE: api/Fact_Historico_Actividad/5
        [HttpDelete("{sk_actividad_id}")]
        public async Task<IActionResult> DeleteFact_Historico_Actividad(int? sk_actividad_id)
        {
            var fact_historico_actividad = await _context.Historico_Actividad.FindAsync(sk_actividad_id);
            if (fact_historico_actividad == null)
            {
                return NotFound();
            }

            _context.Historico_Actividad.Remove(fact_historico_actividad);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool Fact_Historico_ActividadExists(int? sk_actividad_id)
        {
            return _context.Historico_Actividad.Any(e => e.sk_actividad_id == sk_actividad_id);
        }
    }
}