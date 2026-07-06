using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Dim_Usuarios")]
[Route("api/usuarios-dimension")]
[ApiController]
public class UsuariosDimensionController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public UsuariosDimensionController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioProfileDto>>> GetUsuarios()
    {
        return await _context.Usuarios
            .OrderBy(u => u.nombre_usuario)
            .Select(u => new UsuarioProfileDto
            {
                Id = u.sk_usuario_id,
                Nombre = u.nombre_usuario,
                Correo = u.email_usuario,
                RutaImagen = u.ruta_imagen
            })
            .ToListAsync();
    }

    // GET: api/Dim_Usuarios/5
    [HttpGet("{sk_usuario_id}")]
    public async Task<ActionResult<UsuarioProfileDto>> GetUsuario(int sk_usuario_id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.sk_usuario_id == sk_usuario_id)
            .Select(u => new UsuarioProfileDto
            {
                Id = u.sk_usuario_id,
                Nombre = u.nombre_usuario,
                Correo = u.email_usuario,
                RutaImagen = u.ruta_imagen
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound();
        }

        return usuario;
    }

    // PUT: api/Dim_Usuarios/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_usuario_id}")]
    public async Task<IActionResult> PutUsuario(int? sk_usuario_id, Usuario usuario)
    {
        if (sk_usuario_id != usuario.sk_usuario_id)
        {
            return BadRequest();
        }

        _context.Entry(usuario).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UsuarioExists(sk_usuario_id))
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
    public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsuario), new { sk_usuario_id = usuario.sk_usuario_id }, usuario);
    }

    // DELETE: api/Dim_Usuarios/5
    [HttpDelete("{sk_usuario_id}")]
    public async Task<IActionResult> DeleteUsuario(int? sk_usuario_id)
    {
        var usuario = await _context.Usuarios.FindAsync(sk_usuario_id);
        if (usuario == null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool UsuarioExists(int? sk_usuario_id)
    {
        return _context.Usuarios.Any(e => e.sk_usuario_id == sk_usuario_id);
    }
}
