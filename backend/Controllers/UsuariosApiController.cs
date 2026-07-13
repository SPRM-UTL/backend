using backend.Models;
using backend.DTOs; // Acceso a tus DTOs en español
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/UsuariosApi")]
    [Route("api/usuarios")]
    [ApiController]
    public class UsuariosApiController : ControllerBase
    {
        private readonly PruebaaspContext _context;

        public UsuariosApiController(PruebaaspContext context)
        {
            _context = context;
        }

        private const long MaxProfileImageBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedProfileImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        // GET: api/UsuariosApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioProfileDto>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Select(u => new
                UsuarioProfileDto
                {
                    Id = u.sk_usuario_id,
                    Nombre = u.nombre_usuario,
                    Correo = u.email_usuario,
                    RutaImagen = u.ruta_imagen
                })
                .ToListAsync();
        }

        // GET: api/UsuariosApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.sk_usuario_id == id)
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

            return Ok(usuario);
        }

        // PUT: api/UsuariosApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] RegisterDto dto)
        {
            // Nota: Reutilizamos RegisterDto ya que contiene Nombre, Correo y Contrasenia
            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            usuarioExistente.nombre_usuario = dto.Nombre;
            usuarioExistente.email_usuario = dto.Correo;
            if (dto.RutaImagen != null)
            {
                usuarioExistente.ruta_imagen = dto.RutaImagen;
            }

            if (!string.IsNullOrWhiteSpace(dto.Contrasenia))
            {
                var hasher = new PasswordHasher<Usuario>();
                usuarioExistente.contrasenia = hasher.HashPassword(usuarioExistente, dto.Contrasenia);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                mensaje = "Perfil actualizado correctamente"
            });
        }

        // POST: api/UsuariosApi
        [HttpPost]
        public async Task<ActionResult> PostUsuario([FromBody] RegisterDto dto)
        {
            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.email_usuario == dto.Correo);

            if (existeCorreo)
            {
                return BadRequest("El correo ya está registrado");
            }

            var nuevoUsuario = new Usuario
            {
                nombre_usuario = dto.Nombre,
                email_usuario = dto.Correo,
                ruta_imagen = dto.RutaImagen
            };

            var hasher = new PasswordHasher<Usuario>();
            nuevoUsuario.contrasenia = hasher.HashPassword(nuevoUsuario, dto.Contrasenia);

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetUsuario",
                new { id = nuevoUsuario.sk_usuario_id },
                new UsuarioProfileDto
                {
                    Id = nuevoUsuario.sk_usuario_id,
                    Nombre = nuevoUsuario.nombre_usuario,
                    Correo = nuevoUsuario.email_usuario,
                    RutaImagen = nuevoUsuario.ruta_imagen
                }
            );
        }

        [HttpPost("perfil/imagen")]
        [HttpPost("/api/UsuariosDimension/perfil/imagen")]
        [RequestSizeLimit(MaxProfileImageBytes)]
        public async Task<ActionResult> UploadProfileImage(
            [FromForm(Name = "imagen")] IFormFile? imagen,
            [FromForm] int? usuarioId)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return BadRequest("Selecciona una imagen válida.");
            }

            if (imagen.Length > MaxProfileImageBytes)
            {
                return BadRequest("La imagen no debe exceder 5 MB.");
            }

            var extension = Path.GetExtension(imagen.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedProfileImageExtensions.Contains(extension))
            {
                return BadRequest("Solo se permiten imágenes JPG o PNG.");
            }

            if (imagen.ContentType == null || !imagen.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("El archivo enviado no parece ser una imagen.");
            }

            var userIdFromToken = HttpContext.Items["UsuarioId"] as int?;
            var targetUserId = userIdFromToken ?? usuarioId;
            if (targetUserId == null || targetUserId <= 0)
            {
                return BadRequest("No se pudo identificar al usuario.");
            }

            var usuario = await _context.Usuarios.FindAsync(targetUserId.Value);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDirectory = Path.Combine(webRootPath, "uploads", "perfiles", targetUserId.Value.ToString());
            Directory.CreateDirectory(uploadDirectory);

            var safeExtension = extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".jpg";
            var fileName = $"perfil-{Guid.NewGuid():N}{safeExtension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await imagen.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/perfiles/{targetUserId.Value}/{fileName}";
            usuario.ruta_imagen = relativePath;
            await _context.SaveChangesAsync();

            var absoluteUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
            return Ok(new
            {
                ruta_imagen = relativePath,
                url_imagen = absoluteUrl
            });
        }

        // GET: api/usuarios/5/voz-config
        [HttpGet("{id}/voz-config")]
        public async Task<ActionResult<UsuarioVozConfigDto>> GetVozConfig(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            return Ok(new UsuarioVozConfigDto
            {
                ControlVozActivado = usuario.control_voz_activado,
                ConfirmacionHabladaActivada = usuario.confirmacion_hablada_activada,
                VozTipoSeleccionado = usuario.voz_tipo_seleccionado,
                VozVelocidad = usuario.voz_velocidad,
                VozIdioma = usuario.voz_idioma
            });
        }

        // PUT: api/usuarios/5/voz-config
        [HttpPut("{id}/voz-config")]
        public async Task<IActionResult> PutVozConfig(int id, [FromBody] UsuarioVozConfigDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.control_voz_activado = dto.ControlVozActivado;
            usuario.confirmacion_hablada_activada = dto.ConfirmacionHabladaActivada;
            usuario.voz_tipo_seleccionado = dto.VozTipoSeleccionado;
            usuario.voz_velocidad = dto.VozVelocidad;
            usuario.voz_idioma = dto.VozIdioma;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Configuración de voz actualizada" });
        }

        // DELETE: api/UsuariosApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.sk_usuario_id == id);
        }
    }
}
