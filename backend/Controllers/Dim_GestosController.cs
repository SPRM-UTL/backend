using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

[Route("api/[controller]")]
[ApiController]
public class Dim_GestosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    
    // Lista de gestos permitidos
    private static readonly string[] GestosValidos = new[] { 
        "Manos Arriba", "Una Mano Arriba", "Agitar la Mano", "Abrir Puño", "Cerrar Puño" 
    };

    public Dim_GestosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Gestos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dim_Gestos>>> GetDim_Gestos()
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        return await _context.Dim_Gesto
            .Where(g => g.sk_usuario_id == usuarioId)
            .Include(h => h.Historico_Actividad).Include(g => g.Aparato).ToListAsync();
    }

    // GET: api/Dim_Gestos/5
    [HttpGet("{sk_gesto_id}")]
    public async Task<ActionResult<Dim_Gestos>> GetDim_GestoById(int sk_gesto_id)
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

        if (!GestosValidos.Contains(dim_gestos.nombre_gesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        dim_gestos.sk_usuario_id = usuarioId;

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
        if (!GestosValidos.Contains(dim_gestos.nombre_gesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        dim_gestos.sk_usuario_id = usuarioId;

        _context.Dim_Gesto.Add(dim_gestos);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDim_GestoById), new { sk_gesto_id = dim_gestos.sk_gesto_id }, dim_gestos);
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
