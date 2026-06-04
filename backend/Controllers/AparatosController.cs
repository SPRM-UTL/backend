using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Dim_Aparatos")]
[Route("api/aparatos")]
[ApiController]
public class AparatosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    public AparatosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Aparatos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AparatoDto>>> GetAparatos()
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        return await _context.Aparatos
            .Where(a => a.sk_usuario_id == usuarioId)
            .OrderBy(a => a.nombre_aparato)
            .Select(a => new AparatoDto
            {
                SkAparatoId = a.sk_aparato_id,
                NombreAparato = a.nombre_aparato,
                TipoAparato = a.tipo_aparato,
                AccionNombre = a.accion_nombre,
                ComandoBluetooth = a.comando_bluetooth,
                Icono = a.icono,
                MacBluetooth = a.mac_bluetooth,
                NombreBluetooth = a.nombre_bluetooth,
                FechaSincronizacion = a.fecha_sincronizacion
            })
            .ToListAsync();
    }

    // GET: api/Dim_Aparatos/5
    [HttpGet("{sk_aparato_id}")]
    public async Task<ActionResult<AparatoDto>> GetAparatoById(int sk_aparato_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var aparato = await _context.Aparatos
            .Where(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId)
            .Select(a => new AparatoDto
            {
                SkAparatoId = a.sk_aparato_id,
                NombreAparato = a.nombre_aparato,
                TipoAparato = a.tipo_aparato,
                AccionNombre = a.accion_nombre,
                ComandoBluetooth = a.comando_bluetooth,
                Icono = a.icono,
                MacBluetooth = a.mac_bluetooth,
                NombreBluetooth = a.nombre_bluetooth,
                FechaSincronizacion = a.fecha_sincronizacion
            })
            .FirstOrDefaultAsync();

        if (aparato == null)
        {
            return NotFound();
        }

        return aparato;
    }

    // PUT: api/Dim_Aparatos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_aparato_id}")]
    public async Task<IActionResult> PutAparato(int? sk_aparato_id, AparatoDto dto)
    {
        if (sk_aparato_id != dto.SkAparatoId)
        {
            return BadRequest();
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var aparato = await _context.Aparatos
            .FirstOrDefaultAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId);

        if (aparato == null)
        {
            return NotFound();
        }

        ApplyDto(aparato, dto);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AparatoExists(sk_aparato_id))
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
    public async Task<ActionResult<AparatoDto>> PostAparato(AparatoDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var aparato = new Aparato { sk_usuario_id = usuarioId };
        ApplyDto(aparato, dto);

        _context.Aparatos.Add(aparato);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAparatoById), new { sk_aparato_id = aparato.sk_aparato_id }, ToDto(aparato));
    }

    // DELETE: api/Dim_Aparatos/5
    [HttpDelete("{sk_aparato_id}")]
    public async Task<IActionResult> DeleteAparato(int? sk_aparato_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var aparato = await _context.Aparatos
            .FirstOrDefaultAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId);
        if (aparato == null)
        {
            return NotFound();
        }

        _context.Aparatos.Remove(aparato);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool AparatoExists(int? sk_aparato_id)
    {
        return _context.Aparatos.Any(e => e.sk_aparato_id == sk_aparato_id);
    }

    private static AparatoDto ToDto(Aparato aparato) => new()
    {
        SkAparatoId = aparato.sk_aparato_id,
        NombreAparato = aparato.nombre_aparato,
        TipoAparato = aparato.tipo_aparato,
        AccionNombre = aparato.accion_nombre,
        ComandoBluetooth = aparato.comando_bluetooth,
        Icono = aparato.icono,
        MacBluetooth = aparato.mac_bluetooth,
        NombreBluetooth = aparato.nombre_bluetooth,
        FechaSincronizacion = aparato.fecha_sincronizacion
    };

    private static void ApplyDto(Aparato aparato, AparatoDto dto)
    {
        aparato.nombre_aparato = dto.NombreAparato;
        aparato.tipo_aparato = dto.TipoAparato;
        aparato.accion_nombre = dto.AccionNombre;
        aparato.comando_bluetooth = dto.ComandoBluetooth;
        aparato.icono = dto.Icono;
        aparato.mac_bluetooth = dto.MacBluetooth;
        aparato.nombre_bluetooth = dto.NombreBluetooth;
        aparato.fecha_sincronizacion = dto.FechaSincronizacion;
    }
}
