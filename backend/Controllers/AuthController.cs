using backend.Models;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Apis.Auth;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PruebaaspContext _context;
        private readonly IWebHostEnvironment _env;

        public AuthController(PruebaaspContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        [HttpPost("google-login")]
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "505787815686-lqbcfreejrl7ilnkt8hnrfq43gshded6.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

                var usu = await _context.Usuarios.FirstOrDefaultAsync(u => u.email_usuario == payload.Email);
                bool isNewUser = false;
                
                if (usu == null)
                {
                    usu = new Usuario
                    {
                        nombre_usuario = payload.Name,
                        email_usuario = payload.Email,
                        google_id = payload.Subject,
                        control_voz_activado = true,
                        confirmacion_hablada_activada = true,
                        voz_idioma = "es-MX",
                        voz_velocidad = 1.0m
                    };
                    
                    var hasher = new PasswordHasher<Usuario>();
                    usu.contrasenia = hasher.HashPassword(usu, Guid.NewGuid().ToString());
                    
                    _context.Usuarios.Add(usu);
                    isNewUser = true;
                }
                else
                {
                    usu.google_id = payload.Subject;
                }

                // Descargar imagen
                if (!string.IsNullOrEmpty(payload.Picture))
                {
                    string webRootPath = _env.WebRootPath;
                    if (string.IsNullOrEmpty(webRootPath))
                    {
                        webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    }
                    string imagesDir = Path.Combine(webRootPath, "images", "profiles");
                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);
                    }

                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(payload.Picture);
                    string fileName = $"google_{payload.Subject}.jpg";
                    string filePath = Path.Combine(imagesDir, fileName);
                    
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    usu.ruta_imagen = $"/images/profiles/{fileName}";
                }

                await _context.SaveChangesAsync();

                var numeros = RandomNumberGenerator.GetBytes(32);
                string tokenCadena = Convert.ToBase64String(numeros);

                Token n_token = new Token
                {
                    Cadena = tokenCadena,
                    sk_usuario_id = usu.sk_usuario_id,
                    FechaExpiracion = DateTime.Now.AddDays(7), // Google login usually longer duration
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
            catch (InvalidJwtException)
            {
                return Unauthorized("El token de Google es inválido o ha expirado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
