using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PruebaaspContext _context;

        public AuthController(PruebaaspContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            var usu = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.email_usuario == dto.Correo);

            if (usu == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var hasher = new PasswordHasher<Usuario>();
            var resultado = hasher.VerifyHashedPassword(
                usu,
                usu.contrasenia ?? "",
                dto.Contrasenia ?? ""
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return NotFound("Credenciales inválidas");
            }

            var numeros = RandomNumberGenerator.GetBytes(32);
            string tokenCadena = Convert.ToBase64String(numeros);

            Token n_token = new Token
            {
                Cadena = tokenCadena,
                sk_usuario_id = usu.sk_usuario_id,
                FechaExpiracion = DateTime.Now.AddMinutes(30),
                Activo = true
            };

            _context.Token.Add(n_token);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                Id = usu.sk_usuario_id,
                Nombre = usu.nombre_usuario,
                Token = tokenCadena
            });
        }

        [HttpPost("register")]
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

            return Ok(new RegisterResponseDto { Mensaje = "Usuario registrado correctamente" });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var authorization = Request.Headers["Authorization"].ToString();
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                authorization = authorization.Substring(7).Trim();
            }

            var token = await _context.Token
                .FirstOrDefaultAsync(t => t.Cadena == authorization && t.Activo);

            if (token != null)
            {
                token.Activo = false;
                token.FechaBaja = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
