using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

[Route("api/[controller]")]
[ApiController]
public class AparatoTiposController : ControllerBase
{
    private readonly PruebaaspContext _context;

    public AparatoTiposController(PruebaaspContext context)
    {
        _context = context;
    }

    // GET: api/AparatoTipos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AparatoTipoDto>>> GetAparatoTipos()
    {
        return await _context.AparatoTipos
            .OrderByDescending(t => t.es_asistente) // El asistente va primero
            .ThenBy(t => t.nombre_tipo)
            .Select(t => new AparatoTipoDto
            {
                SkAparatoTipoId = t.sk_aparato_tipo_id,
                NombreTipo = t.nombre_tipo,
                Icono = t.icono,
                EsAsistente = t.es_asistente,
                SoportaBluetooth = t.soporta_bluetooth,
                SoportaWifi = t.soporta_wifi,
                PalabrasClaveBusqueda = t.palabras_clave_busqueda
            })
            .ToListAsync();
    }
}
