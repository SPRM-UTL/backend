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
                    Correo = u.email_usuario
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
                    Correo = u.email_usuario
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
                email_usuario = dto.Correo
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
                    Correo = nuevoUsuario.email_usuario
                }
            );
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
