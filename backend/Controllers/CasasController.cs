using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Casas")]
[ApiController]
public class CasasController : ControllerBase
{
    private readonly PruebaaspContext _context;

    public CasasController(PruebaaspContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CasaDto>>> GetCasas()
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casas = await _context.Casas
            .Include(c => c.Habitaciones)
            .Where(c => c.sk_usuario_id == usuarioId)
            .Select(c => new CasaDto
            {
                SkCasaId = c.sk_casa_id,
                NombreCasa = c.nombre_casa,
                SkUsuarioId = c.sk_usuario_id,
                Habitaciones = c.Habitaciones!.Select(h => new HabitacionDto
                {
                    SkHabitacionId = h.sk_habitacion_id,
                    NombreHabitacion = h.nombre_habitacion,
                    SkCasaId = h.sk_casa_id
                }).ToList()
            })
            .ToListAsync();

        return Ok(casas);
    }

    [HttpPost]
    public async Task<ActionResult<CasaDto>> CreateCasa([FromBody] CreateCasaDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casa = new Casa
        {
            nombre_casa = dto.NombreCasa,
            sk_usuario_id = usuarioId.Value
        };

        _context.Casas.Add(casa);
        await _context.SaveChangesAsync();

        return Created("", new CasaDto
        {
            SkCasaId = casa.sk_casa_id,
            NombreCasa = casa.nombre_casa,
            SkUsuarioId = casa.sk_usuario_id
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCasa(int id, [FromBody] CreateCasaDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casa = await _context.Casas.FirstOrDefaultAsync(c => c.sk_casa_id == id && c.sk_usuario_id == usuarioId);
        if (casa == null) return NotFound(new { message = "Casa no encontrada" });

        casa.nombre_casa = dto.NombreCasa;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Casa actualizada" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCasa(int id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casa = await _context.Casas.FirstOrDefaultAsync(c => c.sk_casa_id == id && c.sk_usuario_id == usuarioId);
        if (casa == null) return NotFound(new { message = "Casa no encontrada" });

        _context.Casas.Remove(casa);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Casa eliminada" });
    }
}
