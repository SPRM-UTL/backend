using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Protocol;
using System.Security.Cryptography;

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
        public async Task<ActionResult<Token>> Login(Usuario usuario)
        {
            var usu = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == usuario.Correo);

            if (usu == null) { 
                return NotFound("Usuario no encontrado");
            }

            var hasher = new PasswordHasher<Usuario>();

            var resultado = hasher.VerifyHashedPassword(
                usu,
                usu.Contrasenia,
                usuario.Contrasenia
            );

            if(resultado == PasswordVerificationResult.Failed)
            {
                return NotFound("Credenciales inválidas");
            }

            var numeros = RandomNumberGenerator.GetBytes(32);

            string token = Convert.ToBase64String(numeros);

            Token n_token = new Token();
            n_token.Cadena = token;
            n_token.UsuarioId = usu.Id;
            n_token.FechaExpiracion = DateTime.Now.AddMinutes(30);

            _context.Token.Add(n_token);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                token
            });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var authorization = Request.Headers["Authorization"].ToString();
            var token = await _context.Token
                .FirstOrDefaultAsync(t => t.Cadena == authorization);

            _context.Token.Remove(token);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo);

            if (existeCorreo)
            {
                return BadRequest("El correo ya está registrado");
            }

            var hasher = new PasswordHasher<Usuario>();

            usuario.Contrasenia = hasher.HashPassword(
                usuario,
                usuario.Contrasenia
            );

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje = "Usuario registrado correctamente"
            });
        }
    }
}
