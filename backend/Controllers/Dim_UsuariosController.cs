using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

[Route("api/[controller]")]
[ApiController]
public class Dim_UsuariosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public Dim_UsuariosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dim_Usuarios>>> GetDim_Usuarios()
    {
        return await _context.Dim_Usuario.Include(h => h.Historico_Actividad).ToListAsync();
    }

    // GET: api/Dim_Usuarios/5
    [HttpGet("{sk_usuario_id}")]
    public async Task<ActionResult<Dim_Usuarios>> GetDim_Usuarios(int sk_usuario_id)
    {
        var dim_usuarios = await _context.Dim_Usuario.FindAsync(sk_usuario_id);

        if (dim_usuarios == null)
        {
            return NotFound();
        }

        return dim_usuarios;
    }

    // PUT: api/Dim_Usuarios/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_usuario_id}")]
    public async Task<IActionResult> PutDim_Usuarios(int? sk_usuario_id, Dim_Usuarios dim_usuarios)
    {
        if (sk_usuario_id != dim_usuarios.sk_usuario_id)
        {
            return BadRequest();
        }

        _context.Entry(dim_usuarios).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!Dim_UsuariosExists(sk_usuario_id))
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

    // POST: api/Dim_Usuarios
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Dim_Usuarios>> PostDim_Usuarios(Dim_Usuarios dim_usuarios)
    {
        _context.Dim_Usuario.Add(dim_usuarios);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetDim_Usuarios", new { sk_usuario_id = dim_usuarios.sk_usuario_id }, dim_usuarios);
    }

    // DELETE: api/Dim_Usuarios/5
    [HttpDelete("{sk_usuario_id}")]
    public async Task<IActionResult> DeleteDim_Usuarios(int? sk_usuario_id)
    {
        var dim_usuarios = await _context.Dim_Usuario.FindAsync(sk_usuario_id);
        if (dim_usuarios == null)
        {
            return NotFound();
        }

        _context.Dim_Usuario.Remove(dim_usuarios);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool Dim_UsuariosExists(int? sk_usuario_id)
    {
        return _context.Dim_Usuario.Any(e => e.sk_usuario_id == sk_usuario_id);
    }
}
