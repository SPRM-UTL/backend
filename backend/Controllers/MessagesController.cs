using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly PruebaaspContext _context;
        public MessagesController(PruebaaspContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageEvent>>> GetMessages()
        {
            return await _context.Esp32Message.Include(message => message.SourceDevice)
        .Include(message => message.TargetDevice)
        .OrderByDescending(message => message.CreatedAtUtc)
        .Take(100)
        .Select(message => new MessageEvent(
            message.Id,
            message.SourceDevice.DeviceKey,
            message.TargetDevice == null ? null : message.TargetDevice.DeviceKey,
            message.Message,
            message.Response,
            message.WasProcessed,
            message.ProcessingError,
            message.CreatedAtUtc))
        .ToListAsync();
        }
    }
}
