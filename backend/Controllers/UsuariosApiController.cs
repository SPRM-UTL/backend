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
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            // Proyectamos a un objeto anónimo manteniendo la estructura JSON que Angular ya consume
            return await _context.Dim_Usuario
                .Select(u => new
                {
                    id = u.sk_usuario_id,
                    nombre = u.nombre_usuario,
                    correo = u.email_usuario
                })
                .ToListAsync();
        }

        // GET: api/UsuariosApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Dim_Usuario
                .Where(u => u.sk_usuario_id == id)
                .Select(u => new
                {
                    id = u.sk_usuario_id,
                    nombre = u.nombre_usuario,
                    correo = u.email_usuario
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
            var usuarioExistente = await _context.Dim_Usuario.FindAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            usuarioExistente.nombre_usuario = dto.Nombre;
            usuarioExistente.email_usuario = dto.Correo;

            if (!string.IsNullOrWhiteSpace(dto.Contrasenia))
            {
                var hasher = new PasswordHasher<Dim_Usuarios>();
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
            var existeCorreo = await _context.Dim_Usuario
                .AnyAsync(u => u.email_usuario == dto.Correo);

            if (existeCorreo)
            {
                return BadRequest("El correo ya está registrado");
            }

            var nuevoUsuario = new Dim_Usuarios
            {
                nombre_usuario = dto.Nombre,
                email_usuario = dto.Correo
            };

            var hasher = new PasswordHasher<Dim_Usuarios>();
            nuevoUsuario.contrasenia = hasher.HashPassword(nuevoUsuario, dto.Contrasenia);

            _context.Dim_Usuario.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetUsuario",
                new { id = nuevoUsuario.sk_usuario_id },
                new { id = nuevoUsuario.sk_usuario_id, nombre = nuevoUsuario.nombre_usuario, correo = nuevoUsuario.email_usuario }
            );
        }

        // DELETE: api/UsuariosApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Dim_Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Dim_Usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Dim_Usuario.Any(e => e.sk_usuario_id == id);
        }
    }
}