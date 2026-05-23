using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

[Route("api/[controller]")]
[ApiController]
public class Dim_AparatosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public Dim_AparatosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Aparatos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dim_Aparatos>>> GetDim_Aparatos()
    {
        return await _context.Dim_Aparato.Include(h => h.Historico_Actividad).ToListAsync();
    }

    // GET: api/Dim_Aparatos/5
    [HttpGet("{sk_aparato_id}")]
    public async Task<ActionResult<Dim_Aparatos>> GetDim_Aparatos(int sk_aparato_id)
    {
        var dim_aparatos = await _context.Dim_Aparato.FindAsync(sk_aparato_id);

        if (dim_aparatos == null)
        {
            return NotFound();
        }

        return dim_aparatos;
    }

    // PUT: api/Dim_Aparatos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_aparato_id}")]
    public async Task<IActionResult> PutDim_Aparatos(int? sk_aparato_id, Dim_Aparatos dim_aparatos)
    {
        if (sk_aparato_id != dim_aparatos.sk_aparato_id)
        {
            return BadRequest();
        }

        _context.Entry(dim_aparatos).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!Dim_AparatosExists(sk_aparato_id))
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

    // POST: api/Dim_Aparatos
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Dim_Aparatos>> PostDim_Aparatos(Dim_Aparatos dim_aparatos)
    {
        _context.Dim_Aparato.Add(dim_aparatos);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDim_Aparatos", new { sk_aparato_id = dim_aparatos.sk_aparato_id }, dim_aparatos);
    }

    // DELETE: api/Dim_Aparatos/5
    [HttpDelete("{sk_aparato_id}")]
    public async Task<IActionResult> DeleteDim_Aparatos(int? sk_aparato_id)
    {
        var dim_aparatos = await _context.Dim_Aparato.FindAsync(sk_aparato_id);
        if (dim_aparatos == null)
        {
            return NotFound();
        }

        _context.Dim_Aparato.Remove(dim_aparatos);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool Dim_AparatosExists(int? sk_aparato_id)
    {
        return _context.Dim_Aparato.Any(e => e.sk_aparato_id == sk_aparato_id);
    }
}
