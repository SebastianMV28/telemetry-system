using Microsoft.AspNetCore.Mvc;
using Backend.Models;

[ApiController]
[Route("")]
public class GpsController : ControllerBase
{
    private static Dictionary<string, List<GpsRequest>> vehicles = new();

    [HttpPost("gps")]
    public IActionResult PostGps([FromBody] GpsRequest request)
    {
        // Validaciones
        if (request == null)
            return BadRequest("Body requerido");

        if (string.IsNullOrEmpty(request.VehicleId))
            return BadRequest("vehicle_id es obligatorio");

        if (request.Lat < -90 || request.Lat > 90)
            return BadRequest("lat inválido");

        if (request.Lng < -180 || request.Lng > 180)
            return BadRequest("lng inválido");

        if (request.Timestamp == default)
            return BadRequest("timestamp inválido");

        // Guardar
        if (!vehicles.ContainsKey(request.VehicleId))
        {
            vehicles[request.VehicleId] = new List<GpsRequest>();
        }

        vehicles[request.VehicleId].Add(request);

        return StatusCode(201, new { message = "Coordenada guardada" });
    }

    [HttpGet("vehicles")]
    public IActionResult GetVehicles()
    {
        var result = vehicles.Select(v =>
        {
            var vehicleId = v.Key;
            var gpsList = v.Value;

            var last = gpsList.Last();
            var now = DateTime.UtcNow;
            var diff = now - last.Timestamp;

            string status;

            //  1. Sin señal
            if (diff.TotalSeconds > 120)
            {
                status = "Sin señal";
            }
            //  2. Si solo hay un dato
            else if (gpsList.Count == 1)
            {
                status = "En movimiento";
            }
            else
            {
                var prev = gpsList[gpsList.Count - 2];

                //  3. Cambió de posición → En movimiento
                if (last.Lat != prev.Lat || last.Lng != prev.Lng)
                {
                    status = "En movimiento";
                }
                //  4. No cambió → Detenido
                else
                {
                    status = "Detenido";
                }
            }

            return new
            {
                vehicle_id = vehicleId,
                last_lat = last.Lat,
                last_lng = last.Lng,
                last_seen = last.Timestamp,
                status = status
            };
        });

        return Ok(result);
    }
}