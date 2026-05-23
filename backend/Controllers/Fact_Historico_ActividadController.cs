using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

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
    public async Task<ActionResult<IEnumerable<Fact_Historico_Actividad>>> GetFact_Historico_Actividad()
    {
        return await _context.Historico_Actividad
            .Include(h => h.Dim_Usuario)
            .Include(h => h.Dim_Gesto)
            .Include(h => h.Dim_Aparato)
            .Include(h => h.Dim_Tiempo)
            .ToListAsync();
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
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
