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
            var usu = await _context.Dim_Usuario
                .FirstOrDefaultAsync(u => u.email_usuario == dto.Correo);

            if (usu == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var hasher = new PasswordHasher<Dim_Usuarios>();
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
                FechaExpiracion = DateTime.Now.AddMinutes(30)
            };

            _context.Token.Add(n_token);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = usu.sk_usuario_id,
                nombre = usu.nombre_usuario,
                token = tokenCadena
            });
        }

        [HttpPost("register")]
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

            return Ok(new { mensaje = "Usuario registrado correctamente" });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var authorization = Request.Headers["Authorization"].ToString();
            var token = await _context.Token
                .FirstOrDefaultAsync(t => t.Cadena == authorization);

            if (token != null)
            {
                _context.Token.Remove(token);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}