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
                TipoAparato = a.Tipo == null ? null : a.Tipo.nombre_tipo,
                AccionNombre = a.Accion == null ? null : a.Accion.accion_nombre,
                ComandoBluetooth = a.Accion == null ? null : a.Accion.comando_bluetooth,
                Icono = a.icono,
                MacBluetooth = a.Bluetooth == null ? null : a.Bluetooth.mac_bluetooth,
                NombreBluetooth = a.Bluetooth == null ? null : a.Bluetooth.nombre_bluetooth,
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
                TipoAparato = a.Tipo == null ? null : a.Tipo.nombre_tipo,
                AccionNombre = a.Accion == null ? null : a.Accion.accion_nombre,
                ComandoBluetooth = a.Accion == null ? null : a.Accion.comando_bluetooth,
                Icono = a.icono,
                MacBluetooth = a.Bluetooth == null ? null : a.Bluetooth.mac_bluetooth,
                NombreBluetooth = a.Bluetooth == null ? null : a.Bluetooth.nombre_bluetooth,
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
            .Include(a => a.Bluetooth)
            .FirstOrDefaultAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId);

        if (aparato == null)
        {
            return NotFound();
        }

        await ApplyDto(aparato, dto);

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
        await ApplyDto(aparato, dto);

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

    [HttpGet("{sk_aparato_id}/configuracion-red")]
    public async Task<ActionResult<AparatoConfiguracionRedDto>> GetConfiguracionRed(int sk_aparato_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var configuracion = await _context.AparatoConfiguracionesRed
            .Where(c => c.sk_aparato_id == sk_aparato_id && c.Aparato!.sk_usuario_id == usuarioId)
            .Select(c => ToDto(c))
            .FirstOrDefaultAsync();

        if (configuracion == null)
        {
            return NotFound();
        }

        return configuracion;
    }

    [HttpPut("{sk_aparato_id}/configuracion-red")]
    public async Task<ActionResult<AparatoConfiguracionRedDto>> PutConfiguracionRed(
        int sk_aparato_id,
        AparatoConfiguracionRedDto dto)
    {
        if (dto.SkAparatoId != 0 && dto.SkAparatoId != sk_aparato_id)
        {
            return BadRequest();
        }

        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var aparatoExists = await _context.Aparatos
            .AnyAsync(a => a.sk_aparato_id == sk_aparato_id && a.sk_usuario_id == usuarioId);

        if (!aparatoExists)
        {
            return NotFound();
        }

        var configuracion = await _context.AparatoConfiguracionesRed
            .FirstOrDefaultAsync(c => c.sk_aparato_id == sk_aparato_id);

        if (configuracion == null)
        {
            configuracion = new AparatoConfiguracionRed
            {
                sk_aparato_id = sk_aparato_id,
                fecha_creacion = DateTime.UtcNow
            };
            _context.AparatoConfiguracionesRed.Add(configuracion);
        }

        ApplyDto(configuracion, dto);
        await _context.SaveChangesAsync();

        return ToDto(configuracion);
    }

    [HttpGet("{sk_aparato_controlador_id}/controles")]
    public async Task<ActionResult<IEnumerable<AparatoControlDto>>> GetControles(int sk_aparato_controlador_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        return await _context.AparatoControles
            .Where(c => c.sk_aparato_controlador_id == sk_aparato_controlador_id &&
                c.Controlador!.sk_usuario_id == usuarioId &&
                c.Controlado!.sk_usuario_id == usuarioId)
            .OrderBy(c => c.sk_aparato_controlado_id)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    [HttpPost("{sk_aparato_controlador_id}/controles")]
    public async Task<ActionResult<AparatoControlDto>> PostControl(
        int sk_aparato_controlador_id,
        AparatoControlDto dto)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var controladoId = dto.SkAparatoControladoId;

        if (controladoId == 0 || controladoId == sk_aparato_controlador_id)
        {
            return BadRequest();
        }

        var aparatosValidos = await _context.Aparatos
            .CountAsync(a => (a.sk_aparato_id == sk_aparato_controlador_id || a.sk_aparato_id == controladoId) &&
                a.sk_usuario_id == usuarioId);

        if (aparatosValidos != 2)
        {
            return NotFound();
        }

        var control = await _context.AparatoControles
            .FirstOrDefaultAsync(c => c.sk_aparato_controlador_id == sk_aparato_controlador_id &&
                c.sk_aparato_controlado_id == controladoId);

        if (control == null)
        {
            control = new AparatoControl
            {
                sk_aparato_controlador_id = sk_aparato_controlador_id,
                sk_aparato_controlado_id = controladoId,
                fecha_creacion = DateTime.UtcNow
            };
            _context.AparatoControles.Add(control);
        }

        control.comando_socket = dto.ComandoSocket;
        control.activo = dto.Activo;

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetControles),
            new { sk_aparato_controlador_id },
            ToDto(control));
    }

    [HttpDelete("{sk_aparato_controlador_id}/controles/{sk_aparato_controlado_id}")]
    public async Task<IActionResult> DeleteControl(
        int sk_aparato_controlador_id,
        int sk_aparato_controlado_id)
    {
        var usuarioId = (int?)HttpContext.Items["UsuarioId"];
        var control = await _context.AparatoControles
            .Include(c => c.Controlador)
            .Include(c => c.Controlado)
            .FirstOrDefaultAsync(c => c.sk_aparato_controlador_id == sk_aparato_controlador_id &&
                c.sk_aparato_controlado_id == sk_aparato_controlado_id &&
                c.Controlador!.sk_usuario_id == usuarioId &&
                c.Controlado!.sk_usuario_id == usuarioId);

        if (control == null)
        {
            return NotFound();
        }

        _context.AparatoControles.Remove(control);
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
        TipoAparato = aparato.Tipo?.nombre_tipo,
        AccionNombre = aparato.Accion?.accion_nombre,
        ComandoBluetooth = aparato.Accion?.comando_bluetooth,
        Icono = aparato.icono,
        MacBluetooth = aparato.Bluetooth?.mac_bluetooth,
        NombreBluetooth = aparato.Bluetooth?.nombre_bluetooth,
        FechaSincronizacion = aparato.fecha_sincronizacion
    };

    private async Task ApplyDto(Aparato aparato, AparatoDto dto)
    {
        aparato.nombre_aparato = dto.NombreAparato;
        aparato.icono = dto.Icono;
        aparato.fecha_sincronizacion = dto.FechaSincronizacion;
        aparato.Tipo = await GetOrCreateTipo(dto.TipoAparato);
        aparato.Accion = await GetOrCreateAccion(dto.AccionNombre, dto.ComandoBluetooth);
        ApplyBluetooth(aparato, dto);
    }

    private async Task<AparatoTipo?> GetOrCreateTipo(string? nombreTipo)
    {
        nombreTipo = Normalize(nombreTipo);
        if (nombreTipo == null)
        {
            return null;
        }

        var tipo = await _context.AparatoTipos.FirstOrDefaultAsync(t => t.nombre_tipo == nombreTipo);
        if (tipo != null)
        {
            return tipo;
        }

        tipo = new AparatoTipo { nombre_tipo = nombreTipo };
        _context.AparatoTipos.Add(tipo);
        return tipo;
    }

    private async Task<AparatoAccion?> GetOrCreateAccion(string? accionNombre, string? comandoBluetooth)
    {
        accionNombre = Normalize(accionNombre);
        comandoBluetooth = Normalize(comandoBluetooth);
        if (accionNombre == null && comandoBluetooth == null)
        {
            return null;
        }

        accionNombre ??= string.Empty;
        var accion = await _context.AparatoAcciones.FirstOrDefaultAsync(a =>
            a.accion_nombre == accionNombre && a.comando_bluetooth == comandoBluetooth);
        if (accion != null)
        {
            return accion;
        }

        accion = new AparatoAccion
        {
            accion_nombre = accionNombre,
            comando_bluetooth = comandoBluetooth
        };
        _context.AparatoAcciones.Add(accion);
        return accion;
    }

    private void ApplyBluetooth(Aparato aparato, AparatoDto dto)
    {
        var macBluetooth = Normalize(dto.MacBluetooth);
        var nombreBluetooth = Normalize(dto.NombreBluetooth);
        if (macBluetooth == null && nombreBluetooth == null)
        {
            if (aparato.Bluetooth != null)
            {
                _context.AparatoBluetooth.Remove(aparato.Bluetooth);
                aparato.Bluetooth = null;
            }

            return;
        }

        aparato.Bluetooth ??= new AparatoBluetooth();
        aparato.Bluetooth.mac_bluetooth = macBluetooth;
        aparato.Bluetooth.nombre_bluetooth = nombreBluetooth;
    }

    private static string? Normalize(string? value)
    {
        value = value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static AparatoConfiguracionRedDto ToDto(AparatoConfiguracionRed configuracion) => new()
    {
        SkAparatoConfiguracionRedId = configuracion.sk_aparato_configuracion_red_id,
        SkAparatoId = configuracion.sk_aparato_id,
        DeviceKey = configuracion.device_key,
        IpAddress = configuracion.ip_address,
        MacAddress = configuracion.mac_address,
        HostName = configuracion.host_name,
        PuertoSocket = configuracion.puerto_socket,
        ProtocoloSocket = configuracion.protocolo_socket,
        RutaSocket = configuracion.ruta_socket,
        Activo = configuracion.activo,
        FechaCreacion = configuracion.fecha_creacion,
        FechaUltimaConexion = configuracion.fecha_ultima_conexion
    };

    private static void ApplyDto(AparatoConfiguracionRed configuracion, AparatoConfiguracionRedDto dto)
    {
        configuracion.device_key = dto.DeviceKey;
        configuracion.ip_address = dto.IpAddress;
        configuracion.mac_address = dto.MacAddress;
        configuracion.host_name = dto.HostName;
        configuracion.puerto_socket = dto.PuertoSocket;
        configuracion.protocolo_socket = dto.ProtocoloSocket;
        configuracion.ruta_socket = dto.RutaSocket;
        configuracion.activo = dto.Activo;
    }

    private static AparatoControlDto ToDto(AparatoControl control) => new()
    {
        SkAparatoControlId = control.sk_aparato_control_id,
        SkAparatoControladorId = control.sk_aparato_controlador_id,
        SkAparatoControladoId = control.sk_aparato_controlado_id,
        ComandoSocket = control.comando_socket,
        Activo = control.activo,
        FechaCreacion = control.fecha_creacion
    };

    [HttpGet("control")]
    public async Task<ActionResult<AparatoControlDto>> ObtenerControl()
    {
        var aparatos = await _context.Aparatos
         .Include(a => a.Tipo)
         .Include(a => a.Accion)
         .Include(a => a.Bluetooth)
         .ToListAsync();

        var aparatosDto = aparatos.Select(a => new AparatoDto
        {
            SkAparatoId = a.sk_aparato_id,
            NombreAparato = a.nombre_aparato,
            TipoAparato = a.Tipo?.nombre_tipo,
            Icono = a.icono
        }).ToList();

        var response = new ControlResponseDto
        {
            Luces = aparatosDto
                .Where(a => a.TipoAparato == "Luz")
                .ToList(),

            Bocinas = aparatosDto
                .Where(a => a.TipoAparato == "Bocina")
                .ToList(),

            Ventiladores = aparatosDto
                .Where(a => a.TipoAparato == "Ventilador")
                .ToList()
        };

        return Ok(response);
       
    }
}
