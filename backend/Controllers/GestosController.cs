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
    
    // Lista de gestos permitidos para que acepte los que me enviaron en la imagen
    private static readonly string[] GestosValidos = new[] { 
        "Manos Arriba",
        "Una Mano Arriba",
        "Agitar la Mano",
        "Abrir Puño",
        "Cerrar Puño",
        "A PULGAR ARRIBA",
        "A PULGAR ABAJO",
        "B CUATRO",
        "D UNO",
        "F OK",
        "I",
        "L",
        "U",
        "V PAZ",
        "W TRES",
        "Y",
        "PUÑO",
        "CINCO MANO ABIERTA",
        "ROCK",
        "TE AMO ILY",
        "DESCONOCIDO"
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
                FraseVozActivadora = g.frase_voz_activadora,
                SkAparatoId = g.sk_aparato_id,
                ContactoOutlet = g.contacto_outlet,
                Pasos = g.PasosSecuencia.Select(p => new GestoPasoDto
                {
                    SkGestoPasoId = p.sk_gesto_paso_id,
                    Orden = p.orden,
                    EsActivador = p.es_activador,
                    NombreGesto = p.nombre_gesto,
                    ManoObjetivo = p.mano_objetivo,
                    CuadrosRequeridos = p.cuadros_requeridos
                }).OrderBy(p => p.Orden).ToList()
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
                FraseVozActivadora = g.frase_voz_activadora,
                SkAparatoId = g.sk_aparato_id,
                ContactoOutlet = g.contacto_outlet,
                Pasos = g.PasosSecuencia.Select(p => new GestoPasoDto
                {
                    SkGestoPasoId = p.sk_gesto_paso_id,
                    Orden = p.orden,
                    EsActivador = p.es_activador,
                    NombreGesto = p.nombre_gesto,
                    ManoObjetivo = p.mano_objetivo,
                    CuadrosRequeridos = p.cuadros_requeridos
                }).OrderBy(p => p.Orden).ToList()
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

        bool isCombo = dto.TipoDisparadorNombre != null && 
                       (dto.TipoDisparadorNombre.ToUpper().Contains("COMBO") || 
                        dto.TipoDisparadorNombre.ToUpper().Contains("SECUENCIA") ||
                        dto.TipoDisparadorNombre.ToUpper().Contains("VOZ"));

        if (!isCombo && !GestosValidos.Contains(dto.NombreGesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = await _context.Gestos
            .Include(g => g.PasosSecuencia)
            .FirstOrDefaultAsync(g => g.sk_gesto_id == sk_gesto_id && g.sk_usuario_id == usuarioId);

        if (gesto == null)
        {
            return NotFound();
        }

        ApplyDto(gesto, dto);

        if (gesto.PasosSecuencia != null)
        {
            _context.GestoPasos.RemoveRange(gesto.PasosSecuencia);
        }
        
        if (dto.Pasos != null && dto.Pasos.Any())
        {
            gesto.PasosSecuencia = dto.Pasos.Select(p => new GestoPaso
            {
                orden = p.Orden,
                es_activador = p.EsActivador,
                nombre_gesto = p.NombreGesto,
                mano_objetivo = p.ManoObjetivo,
                cuadros_requeridos = p.CuadrosRequeridos
            }).ToList();
        }

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
        bool isCombo = dto.TipoDisparadorNombre != null && 
                       (dto.TipoDisparadorNombre.ToUpper().Contains("COMBO") || 
                        dto.TipoDisparadorNombre.ToUpper().Contains("SECUENCIA") ||
                        dto.TipoDisparadorNombre.ToUpper().Contains("VOZ"));

        if (!isCombo && !GestosValidos.Contains(dto.NombreGesto))
        {
            return BadRequest("Gesto no reconocido. Debe seleccionar un gesto válido.");
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var gesto = new Gesto { sk_usuario_id = usuarioId };
        ApplyDto(gesto, dto);

        if (dto.Pasos != null && dto.Pasos.Any())
        {
            gesto.PasosSecuencia = dto.Pasos.Select(p => new GestoPaso
            {
                orden = p.Orden,
                es_activador = p.EsActivador,
                nombre_gesto = p.NombreGesto,
                mano_objetivo = p.ManoObjetivo,
                cuadros_requeridos = p.CuadrosRequeridos
            }).ToList();
        }

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
    // Dentro de GestosController.cs
    [HttpGet("{sk_gesto_id}/detalle")]
    public async Task<ActionResult<GestoDetalleDto>> GetGestoDetalle(int sk_gesto_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];

        var detalle = await _context.GestoDetalles
            .Include(gd => gd.Gesto)
            .Include(gd => gd.MediosReferencia)
            .FirstOrDefaultAsync(gd => gd.GestoId == sk_gesto_id && gd.Gesto.sk_usuario_id == usuarioId);

        if (detalle == null)
        {
            return NotFound("No se encontró el detalle para el gesto especificado.");
        }

        // El mapeo limpio usando tu modelo final:
        var dto = new GestoDetalleDto
        {
            SkGestoDetalleId = detalle.Id, // <--- Conecta con tu modelo
            SkGestoId = detalle.GestoId,   // <--- Conecta con tu modelo
            NombreGesto = detalle.Gesto?.nombre_gesto ?? string.Empty,
            DuracionSegundos = detalle.DuracionSegundos,
            IluminacionRecomendada = detalle.IluminacionRecomendada,
            DistanciaRecomendada = detalle.DistanciaRecomendada,
            MediosReferencia = detalle.MediosReferencia.Select(m => new GestoMediaDto
            {
                SkMediaId = m.Id,
                UrlArchivo = m.UrlArchivo,
                TipoMedia = m.TipoMedia,
                Extension = m.Extension
            }).ToList()
        };

        return Ok(dto);
    }

    // POST: api/gestos/{sk_gesto_id}/detalle
    // Es utilizada para la alimentación de los detalles de los gestos
    [HttpPost("{sk_gesto_id}/detalle")]
    public async Task<ActionResult<GestoDetalleDto>> PostGestoDetalle(int sk_gesto_id, GestoDetalleDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];

        var gesto = await _context.Gestos
            .FirstOrDefaultAsync(g => g.sk_gesto_id == sk_gesto_id && g.sk_usuario_id == usuarioId);

        if (gesto == null)
        {
            return NotFound("El gesto no existe o no tienes permiso para acceder a él.");
        }

        var detalle = await _context.GestoDetalles
            .Include(d => d.MediosReferencia)
            .FirstOrDefaultAsync(d => d.GestoId == sk_gesto_id);

        if (detalle == null)
        {
            detalle = new GestoDetalle { GestoId = sk_gesto_id };
            _context.GestoDetalles.Add(detalle);
        }

        detalle.DuracionSegundos = dto.DuracionSegundos;
        detalle.IluminacionRecomendada = dto.IluminacionRecomendada;
        detalle.DistanciaRecomendada = dto.DistanciaRecomendada;

        if (detalle.MediosReferencia.Any())
        {
            _context.GestoMedias.RemoveRange(detalle.MediosReferencia);
        }

        detalle.MediosReferencia = dto.MediosReferencia.Select(m => new GestoMedia
        {
            UrlArchivo = m.UrlArchivo,
            TipoMedia = m.TipoMedia,
            Extension = m.Extension
        }).ToList();

        await _context.SaveChangesAsync();

        // 5. Devolver la info completa para confirmar
        return Ok(new GestoDetalleDto
        {
            SkGestoDetalleId = detalle.Id,
            SkGestoId = detalle.GestoId,
            NombreGesto = gesto.nombre_gesto,
            DuracionSegundos = detalle.DuracionSegundos,
            IluminacionRecomendada = detalle.IluminacionRecomendada,
            DistanciaRecomendada = detalle.DistanciaRecomendada,
            MediosReferencia = detalle.MediosReferencia.Select(m => new GestoMediaDto
            {
                SkMediaId = m.Id,
                UrlArchivo = m.UrlArchivo,
                TipoMedia = m.TipoMedia,
                Extension = m.Extension
            }).ToList()
        });
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
        FraseVozActivadora = gesto.frase_voz_activadora,
        SkAparatoId = gesto.sk_aparato_id,
        ContactoOutlet = gesto.contacto_outlet,
        Pasos = gesto.PasosSecuencia?.Select(p => new GestoPasoDto
        {
            SkGestoPasoId = p.sk_gesto_paso_id,
            Orden = p.orden,
            EsActivador = p.es_activador,
            NombreGesto = p.nombre_gesto,
            ManoObjetivo = p.mano_objetivo,
            CuadrosRequeridos = p.cuadros_requeridos
        }).OrderBy(p => p.Orden).ToList()
    };

    private static void ApplyDto(Gesto gesto, GestoDto dto)
    {
        gesto.bk_gesto_id = dto.BkGestoId;
        gesto.nombre_gesto = dto.NombreGesto;
        gesto.icono= dto.Icono;
        gesto.identificador_ia = dto.IdentificadorIa;
        gesto.nivel_confianza_minimo = dto.NivelConfianzaMinimo;
        gesto.tipo_disparador_nombre = dto.TipoDisparadorNombre;
        gesto.frase_voz_activadora = dto.FraseVozActivadora;
        gesto.sk_aparato_id = dto.SkAparatoId;
        gesto.contacto_outlet = dto.ContactoOutlet;
    }
}
