using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/catalogo_gestos")]
    [ApiController]
    public class CatalogoGestosController : ControllerBase
    {
        private readonly PruebaaspContext _context;

        public CatalogoGestosController(PruebaaspContext context)
        {
            _context = context;
        }

        // GET: api/catalogo_gestos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogoGestoDto>>> GetCatalogoGestos()
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];
            
            // if (usuarioId == null)
            // {
            //     return Unauthorized(new { message = "Usuario no autenticado." });
            // }

            // Obtener el catálogo y hacer join con la configuración del usuario
            var query = from cg in _context.CatalogoGestos
                        let userConfig = _context.UsuarioGestosConfig.FirstOrDefault(uc => uc.sk_catalogo_gesto_id == cg.sk_catalogo_gesto_id && uc.sk_usuario_id == usuarioId)
                        orderby cg.sk_catalogo_gesto_id
                        select new CatalogoGestoDto
                        {
                            SkCatalogoGestoId = cg.sk_catalogo_gesto_id,
                            Nombre = cg.nombre,
                            Icono = cg.icono,
                            IsBodyGesture = cg.is_body_gesture,
                            // Por defecto true si no hay configuración, de lo contrario lo que esté configurado
                            IsActive = userConfig != null ? userConfig.is_active : true
                        };

            var resultados = await query.ToListAsync();
            return Ok(resultados);
        }

        // POST: api/catalogo_gestos/config
        [HttpPost("config")]
        public async Task<ActionResult> GuardarConfiguracion([FromBody] List<GuardarConfiguracionGestosDto> configuraciones)
        {
            var usuarioId = (int?)HttpContext.Items["UsuarioId"];
            
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            foreach (var conf in configuraciones)
            {
                var existingConfig = await _context.UsuarioGestosConfig
                    .FirstOrDefaultAsync(u => u.sk_usuario_id == usuarioId && u.sk_catalogo_gesto_id == conf.SkCatalogoGestoId);
                
                if (existingConfig != null)
                {
                    existingConfig.is_active = conf.IsActive;
                }
                else
                {
                    _context.UsuarioGestosConfig.Add(new UsuarioGestoConfig
                    {
                        sk_usuario_id = usuarioId.Value,
                        sk_catalogo_gesto_id = conf.SkCatalogoGestoId,
                        is_active = conf.IsActive
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Configuración guardada exitosamente" });
        }
    }
}
