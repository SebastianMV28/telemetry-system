using Microsoft.AspNetCore.Mvc;
using Backend.Models;

[ApiController]
[Route("")]
public class GpsController : ControllerBase
{
    private static Dictionary<string, List<GpsRequest>> vehicles = new();
    //Metodo POST
    [HttpPost("gps")]
    public IActionResult PostGps([FromBody] GpsRequest request)
    {
        request.Timestamp = DateTime.UtcNow;
        // Validaciones

        //Valida que el body de la solicitud este completo
        if (request == null)
            return BadRequest("Body requerido");
        //Valida el ingreso del id para la solicitud
        if (string.IsNullOrEmpty(request.VehicleId))
            return BadRequest("vehicle_id es obligatorio");
        //Valida que el valor de latencia este dentro de los parametros correctos
        if (request.Lat < -90 || request.Lat > 90)
            return BadRequest("lat inválido");
        //Valida que el valor de longitud este dentro de los parametros correctos
        if (request.Lng < -180 || request.Lng > 180)
            return BadRequest("lng inválido");
        //Valida la fecha y hora no sean iguales al valor por defecto
        if (request.Timestamp == default)
            return BadRequest("timestamp inválido");

        // Guardar
        //Si la solicitud del vehiculo no se encuentra en el dictionario lo agrega como un nuevo objeto.
        if (!vehicles.ContainsKey(request.VehicleId))
        {
            vehicles[request.VehicleId] = new List<GpsRequest>();
        }
        //Aca agrega la solicitud a la lista
        vehicles[request.VehicleId].Add(request);

        return StatusCode(201, new { message = "Coordenada guardada" });
    }
    //Metodo GET
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

            if (diff.TotalSeconds <= 60)
            {
                // Datos recientes → evaluar movimiento

                if (gpsList.Count == 1)
                {
                    status = "En movimiento";
                }
                else
                {
                    var prev = gpsList[gpsList.Count - 2];

                    if (last.Lat != prev.Lat || last.Lng != prev.Lng)
                    {
                        status = "En movimiento";
                    }
                    else
                    {
                        status = "Detenido";
                    }
                }
            }
            else if (diff.TotalSeconds <= 120)
            {
                // Entre 1 y 2 minutos
                status = "Detenido";
            }
            else
            {
                // Más de 2 minutos
                status = "Sin señal";
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
    //Metodo DELETE
    // Define un endpoint HTTP DELETE en la ruta: /vehicles/{id}
    // Por ejemplo: DELETE /vehicles/VH-001
    [HttpDelete("vehicles/{id}")]
    public IActionResult DeleteVehicle(string id)
    {
        //Verifica si el vehículo existe en el Dictionary
        //ContainsKey busca si existe una clave (vehicleId)
        if (!vehicles.ContainsKey(id))
        {
            //Si no existe, devuelve 404 Not Found
            //Esto indica al cliente que el vehículo no fue encontrado
            return NotFound("Vehículo no encontrado");
        }

        //Si existe, lo elimina del Dictionary
        //Esto borra completamente el historial del vehículo
        vehicles.Remove(id);

        //Devuelve 204 No Content
        //Significa: "la operación fue exitosa pero no hay contenido que devolver"
        return NoContent();
    }



}