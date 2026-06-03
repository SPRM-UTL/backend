using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly PruebaaspContext _context;
        public DevicesController(PruebaaspContext context)
        {
            _context = context;
        }

        // GET: api/Devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevices()
        {
            return await _context.Esp32Device
                .OrderBy(device => device.Name)
                .Select(device => new DeviceDto(
                    device.Id,
                    device.DeviceKey,
                    device.Name,
                    device.Description,
                    device.IsActive,
                    device.LastSeenAtUtc
                ))
                .ToListAsync();
        }

        // POST: api/Devices
        [HttpPost]
        public async Task<ActionResult<Esp32Device>> PostDevices(Esp32Device device)
        {

            if (string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.DeviceKey))
            {
                return BadRequest("El nombre y el device_key son obligatorios.");
            }

            var exists = await _context.Esp32Device.AnyAsync(d => d.DeviceKey == device.DeviceKey);

            if (exists)
            {
                return Conflict("Ya existe un dispositivo con ese DeviceKey.");
            }

            _context.Esp32Device.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDevices", new { Id = device.Id }, device);
        }
    }
}
