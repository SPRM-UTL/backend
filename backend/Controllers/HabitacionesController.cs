using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Habitaciones")]
[ApiController]
public class HabitacionesController : ControllerBase
{
    private readonly PruebaaspContext _context;

    public HabitacionesController(PruebaaspContext context)
    {
        _context = context;
    }

    [HttpGet("Casa/{casaId}")]
    public async Task<ActionResult<IEnumerable<HabitacionDto>>> GetHabitacionesByCasa(int casaId)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casa = await _context.Casas.FirstOrDefaultAsync(c => c.sk_casa_id == casaId && c.sk_usuario_id == usuarioId);
        if (casa == null) return NotFound(new { message = "Casa no encontrada o no pertenece al usuario" });

        var habitaciones = await _context.Habitaciones
            .Include(h => h.Aparatos)
            .Where(h => h.sk_casa_id == casaId)
            .Select(h => new HabitacionDto
            {
                SkHabitacionId = h.sk_habitacion_id,
                NombreHabitacion = h.nombre_habitacion,
                SkCasaId = h.sk_casa_id,
                Aparatos = h.Aparatos == null ? null : h.Aparatos.Select(a => new AparatoDto
                {
                    SkAparatoId = a.sk_aparato_id,
                    NombreAparato = a.nombre_aparato,
                    Icono = a.icono,
                    SkHabitacionId = a.sk_habitacion_id
                }).ToList()
            })
            .ToListAsync();

        return Ok(habitaciones);
    }

    [HttpPost]
    public async Task<ActionResult<HabitacionDto>> CreateHabitacion([FromBody] CreateHabitacionDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var casa = await _context.Casas.FirstOrDefaultAsync(c => c.sk_casa_id == dto.SkCasaId && c.sk_usuario_id == usuarioId);
        if (casa == null) return NotFound(new { message = "Casa no encontrada o no pertenece al usuario" });

        var habitacion = new Habitacion
        {
            nombre_habitacion = dto.NombreHabitacion,
            sk_casa_id = dto.SkCasaId
        };

        _context.Habitaciones.Add(habitacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHabitacionesByCasa), new { casaId = habitacion.sk_casa_id }, new HabitacionDto
        {
            SkHabitacionId = habitacion.sk_habitacion_id,
            NombreHabitacion = habitacion.nombre_habitacion,
            SkCasaId = habitacion.sk_casa_id
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHabitacion(int id, [FromBody] CreateHabitacionDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var habitacion = await _context.Habitaciones
            .Include(h => h.Casa)
            .FirstOrDefaultAsync(h => h.sk_habitacion_id == id);

        if (habitacion == null || habitacion.Casa?.sk_usuario_id != usuarioId) 
            return NotFound(new { message = "Habitación no encontrada" });

        habitacion.nombre_habitacion = dto.NombreHabitacion;
        if (dto.SkCasaId > 0 && dto.SkCasaId != habitacion.sk_casa_id)
        {
             var nuevaCasa = await _context.Casas.FirstOrDefaultAsync(c => c.sk_casa_id == dto.SkCasaId && c.sk_usuario_id == usuarioId);
             if (nuevaCasa != null) habitacion.sk_casa_id = dto.SkCasaId;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabitacion(int id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        if (usuarioId == null) return Unauthorized(new { message = "Usuario no autenticado" });

        var habitacion = await _context.Habitaciones
            .Include(h => h.Casa)
            .FirstOrDefaultAsync(h => h.sk_habitacion_id == id);

        if (habitacion == null || habitacion.Casa?.sk_usuario_id != usuarioId) 
            return NotFound(new { message = "Habitación no encontrada" });

        _context.Habitaciones.Remove(habitacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
