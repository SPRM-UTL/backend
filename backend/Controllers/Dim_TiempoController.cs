using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

[Route("api/[controller]")]
[ApiController]
public class Dim_TiempoController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public Dim_TiempoController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Tiempo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dim_Tiempo>>> GetDim_Tiempo()
    {
        return await _context.Dim_Tiempo.Include(h => h.Historico_Actividad).ToListAsync();
    }

    // GET: api/Dim_Tiempo/5
    [HttpGet("{sk_tiempo_id}")]
    public async Task<ActionResult<Dim_Tiempo>> GetDim_Tiempo(int sk_tiempo_id)
    {
        var dim_tiempo = await _context.Dim_Tiempo.FindAsync(sk_tiempo_id);

        if (dim_tiempo == null)
        {
            return NotFound();
        }

        return dim_tiempo;
    }

    // PUT: api/Dim_Tiempo/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_tiempo_id}")]
    public async Task<IActionResult> PutDim_Tiempo(int? sk_tiempo_id, Dim_Tiempo dim_tiempo)
    {
        if (sk_tiempo_id != dim_tiempo.sk_tiempo_id)
        {
            return BadRequest();
        }

        _context.Entry(dim_tiempo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!Dim_TiempoExists(sk_tiempo_id))
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
    public async Task<ActionResult<Dim_Tiempo>> PostDim_Tiempo(Dim_Tiempo dim_tiempo)
    {
        _context.Dim_Tiempo.Add(dim_tiempo);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDim_Tiempo", new { sk_tiempo_id = dim_tiempo.sk_tiempo_id }, dim_tiempo);
    }

    // DELETE: api/Dim_Tiempo/5
    [HttpDelete("{sk_tiempo_id}")]
    public async Task<IActionResult> DeleteDim_Tiempo(int? sk_tiempo_id)
    {
        var dim_tiempo = await _context.Dim_Tiempo.FindAsync(sk_tiempo_id);
        if (dim_tiempo == null)
        {
            return NotFound();
        }

        _context.Dim_Tiempo.Remove(dim_tiempo);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool Dim_TiempoExists(int? sk_tiempo_id)
    {
        return _context.Dim_Tiempo.Any(e => e.sk_tiempo_id == sk_tiempo_id);
    }
}
