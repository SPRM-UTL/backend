using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Dim_Tiempo")]
[Route("api/tiempo")]
[ApiController]
public class TiempoController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public TiempoController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Tiempo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TiempoDto>>> GetTiempos()
    {
        return await _context.Tiempos
            .OrderBy(t => t.fecha_completa)
            .ThenBy(t => t.hora_periodo)
            .Select(t => new TiempoDto
            {
                SkTiempoId = t.sk_tiempo_id,
                FechaCompleta = t.fecha_completa,
                Anio = t.anio,
                MesNumero = t.mes_numero,
                MesNombre = t.mes_nombre,
                DiaSemanaNombre = t.dia_semana_nombre,
                HoraPeriodo = t.hora_periodo
            })
            .ToListAsync();
    }

    // GET: api/Dim_Tiempo/5
    [HttpGet("{sk_tiempo_id}")]
    public async Task<ActionResult<TiempoDto>> GetTiempo(int sk_tiempo_id)
    {
        var tiempo = await _context.Tiempos
            .Where(t => t.sk_tiempo_id == sk_tiempo_id)
            .Select(t => new TiempoDto
            {
                SkTiempoId = t.sk_tiempo_id,
                FechaCompleta = t.fecha_completa,
                Anio = t.anio,
                MesNumero = t.mes_numero,
                MesNombre = t.mes_nombre,
                DiaSemanaNombre = t.dia_semana_nombre,
                HoraPeriodo = t.hora_periodo
            })
            .FirstOrDefaultAsync();

        if (tiempo == null)
        {
            return NotFound();
        }

        return tiempo;
    }

    // PUT: api/Dim_Tiempo/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_tiempo_id}")]
    public async Task<IActionResult> PutTiempo(int? sk_tiempo_id, Tiempo tiempo)
    {
        if (sk_tiempo_id != tiempo.sk_tiempo_id)
        {
            return BadRequest();
        }

        _context.Entry(tiempo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TiempoExists(sk_tiempo_id))
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

    // POST: api/Dim_Tiempo
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Tiempo>> PostTiempo(Tiempo tiempo)
    {
        _context.Tiempos.Add(tiempo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTiempo), new { sk_tiempo_id = tiempo.sk_tiempo_id }, tiempo);
    }

    // DELETE: api/Dim_Tiempo/5
    [HttpDelete("{sk_tiempo_id}")]
    public async Task<IActionResult> DeleteTiempo(int? sk_tiempo_id)
    {
        var tiempo = await _context.Tiempos.FindAsync(sk_tiempo_id);
        if (tiempo == null)
        {
            return NotFound();
        }

        _context.Tiempos.Remove(tiempo);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool TiempoExists(int? sk_tiempo_id)
    {
        return _context.Tiempos.Any(e => e.sk_tiempo_id == sk_tiempo_id);
    }
}
