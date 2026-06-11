using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/Dim_Gestos")]
[Route("api/gestos")]
[ApiController]
public class GestosController : ControllerBase
{
    private readonly PruebaaspContext _context;
    
    // Lista de gestos permitidos
    private static readonly string[] GestosValidos = new[] { 
        "Manos Arriba", "Una Mano Arriba", "Agitar la Mano", "Abrir Puño", "Cerrar Puño" 
    };

    public GestosController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/Dim_Gestos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GestoDto>>> GetGestos()
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        return await _context.Gestos
            .Where(g => g.sk_usuario_id == usuarioId)
            .OrderBy(g => g.nombre_gesto)
            .Select(g => new GestoDto
            {
                SkGestoId = g.sk_gesto_id,
                BkGestoId = g.bk_gesto_id,
                NombreGesto = g.nombre_gesto,
                Icono = g.icono,
                IdentificadorIa = g.identificador_ia,
                NivelConfianzaMinimo = g.nivel_confianza_minimo,
                TipoDisparadorNombre = g.tipo_disparador_nombre,
                SkAparatoId = g.sk_aparato_id
            })
            .ToListAsync();
    }

    // GET: api/Dim_Gestos/5
    [HttpGet("{sk_gesto_id}")]
    public async Task<ActionResult<GestoDto>> GetGestoById(int sk_gesto_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = await _context.Gestos
            .Where(g => g.sk_gesto_id == sk_gesto_id && g.sk_usuario_id == usuarioId)
            .Select(g => new GestoDto
            {
                SkGestoId = g.sk_gesto_id,
                BkGestoId = g.bk_gesto_id,
                NombreGesto = g.nombre_gesto,
                Icono = g.icono,
                IdentificadorIa = g.identificador_ia,
                NivelConfianzaMinimo = g.nivel_confianza_minimo,
                TipoDisparadorNombre = g.tipo_disparador_nombre,
                SkAparatoId = g.sk_aparato_id
            })
            .FirstOrDefaultAsync();

        if (gesto == null)
        {
            return NotFound();
        }

        return gesto;
    }

    // PUT: api/Dim_Gestos/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sk_gesto_id}")]
    public async Task<IActionResult> PutGesto(int? sk_gesto_id, GestoDto dto)
    {
        if (sk_gesto_id != dto.SkGestoId)
        {
            return BadRequest();
        }

        if (!GestosValidos.Contains(dto.NombreGesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = await _context.Gestos
            .FirstOrDefaultAsync(g => g.sk_gesto_id == sk_gesto_id && g.sk_usuario_id == usuarioId);

        if (gesto == null)
        {
            return NotFound();
        }

        ApplyDto(gesto, dto);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GestoExists(sk_gesto_id))
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
    public async Task<ActionResult<GestoDto>> PostGesto(GestoDto dto)
    {
        if (!GestosValidos.Contains(dto.NombreGesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = new Gesto { sk_usuario_id = usuarioId };
        ApplyDto(gesto, dto);

        _context.Gestos.Add(gesto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGestoById), new { sk_gesto_id = gesto.sk_gesto_id }, ToDto(gesto));
    }

    // DELETE: api/Dim_Gestos/5
    [HttpDelete("{sk_gesto_id}")]
    public async Task<IActionResult> DeleteGesto(int? sk_gesto_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = await _context.Gestos
            .FirstOrDefaultAsync(g => g.sk_gesto_id == sk_gesto_id && g.sk_usuario_id == usuarioId);
        if (gesto == null)
        {
            return NotFound();
        }

        _context.Gestos.Remove(gesto);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private bool GestoExists(int? sk_gesto_id)
    {
        return _context.Gestos.Any(e => e.sk_gesto_id == sk_gesto_id);
    }

    private static GestoDto ToDto(Gesto gesto) => new()
    {
        SkGestoId = gesto.sk_gesto_id,
        BkGestoId = gesto.bk_gesto_id,
        NombreGesto = gesto.nombre_gesto,
        Icono = gesto.icono,
        IdentificadorIa = gesto.identificador_ia,
        NivelConfianzaMinimo = gesto.nivel_confianza_minimo,
        TipoDisparadorNombre = gesto.tipo_disparador_nombre,
        SkAparatoId = gesto.sk_aparato_id
    };

    private static void ApplyDto(Gesto gesto, GestoDto dto)
    {
        gesto.bk_gesto_id = dto.BkGestoId;
        gesto.nombre_gesto = dto.NombreGesto;
        gesto.icono= dto.Icono;
        gesto.identificador_ia = dto.IdentificadorIa;
        gesto.nivel_confianza_minimo = dto.NivelConfianzaMinimo;
        gesto.tipo_disparador_nombre = dto.TipoDisparadorNombre;
        gesto.sk_aparato_id = dto.SkAparatoId;
    }
}
