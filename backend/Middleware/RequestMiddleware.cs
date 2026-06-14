using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace backend.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, PruebaaspContext db)
        {
            
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Request.Path.Equals("/api/Auth/login") &&
                    !context.Request.Path.Equals("/api/Auth/register"))
                {
                    var authorization = context.Request.Headers["Authorization"].ToString();

                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        authorization = authorization.Substring(7).Trim();
                    }

                    var token = await db.Token
                        .FirstOrDefaultAsync(t => t.Cadena == authorization && t.Activo);

                    if (token == null || token.FechaExpiracion < DateTime.Now)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("No autorizado");
                        return;
                    }

                    context.Items["UsuarioId"] = token.sk_usuario_id;
                    token.FechaExpiracion = DateTime.Now.AddMonths(1);
                    await db.SaveChangesAsync();
                } 

                var originalBodyStream = context.Response.Body;

                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                responseBody.Seek(0, SeekOrigin.Begin);

                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                responseBody.Seek(0, SeekOrigin.Begin);

                object? data = null;

                try
                {
                    data = JsonSerializer.Deserialize<object>(responseText);
                }
                catch
                {
                    data = responseText;
                }

                var responseFormat = new
                {
                    success = context.Response.StatusCode >= 200 &&
                              context.Response.StatusCode < 300,

                    status = context.Response.StatusCode,

                    data = data
                };

                var jsonResponse = JsonSerializer.Serialize(responseFormat);

                context.Response.ContentType = "application/json";

                var bytes = Encoding.UTF8.GetBytes(jsonResponse);

                context.Response.ContentLength = bytes.Length;

                await originalBodyStream.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
