using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

[Route("api/[controller]")]
[ApiController]
public class Dim_GestosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public Dim_GestosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Gestos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dim_Gestos>>> GetDim_Gestos()
    {
        return await _context.Dim_Gesto.Include(h => h.Historico_Actividad).ToListAsync();
    }

    // GET: api/Dim_Gestos/5
    [HttpGet("{sk_gesto_id}")]
    public async Task<ActionResult<Dim_Gestos>> GetDim_Gestos(int sk_gesto_id)
    {
        var dim_gestos = await _context.Dim_Gesto.FindAsync(sk_gesto_id);

        if (dim_gestos == null)
        {
            return NotFound();
        }

        return dim_gestos;
    }

    // PUT: api/Dim_Gestos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_gesto_id}")]
    public async Task<IActionResult> PutDim_Gestos(int? sk_gesto_id, Dim_Gestos dim_gestos)
    {
        if (sk_gesto_id != dim_gestos.sk_gesto_id)
        {
            return BadRequest();
        }

        _context.Entry(dim_gestos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!Dim_GestosExists(sk_gesto_id))
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

    // POST: api/Dim_Gestos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Dim_Gestos>> PostDim_Gestos(Dim_Gestos dim_gestos)
    {
        _context.Dim_Gesto.Add(dim_gestos);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDim_Gestos", new { sk_gesto_id = dim_gestos.sk_gesto_id }, dim_gestos);
    }

    // DELETE: api/Dim_Gestos/5
    [HttpDelete("{sk_gesto_id}")]
    public async Task<IActionResult> DeleteDim_Gestos(int? sk_gesto_id)
    {
        var dim_gestos = await _context.Dim_Gesto.FindAsync(sk_gesto_id);
        if (dim_gestos == null)
        {
            return NotFound();
        }

        _context.Dim_Gesto.Remove(dim_gestos);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool Dim_GestosExists(int? sk_gesto_id)
    {
        return _context.Dim_Gesto.Any(e => e.sk_gesto_id == sk_gesto_id);
    }
}
