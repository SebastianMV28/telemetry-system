using System.Text;
using System.Text.Json;

// HttpClient se utiliza para enviar solicitudes HTTP al backend
var httpClient = new HttpClient();

// Lista de vehículos a simular.
// La prueba requiere mínimo 3 vehículos.
var vehicles = new List<string> { "VH-001", "VH-002", "VH-003","VH-004","VH-005" };

// Random se usa para generar coordenadas aleatorias e inyectar errores
var random = new Random();

// Bucle infinito: el simulador seguirá enviando datos hasta detenerlo manualmente
while (true)
{
    // Envía una solicitud GPS por cada vehículo de la lista
    foreach (var vehicle in vehicles)
    {
        await SendGps(vehicle);
    }

    // Espera 5 segundos antes de enviar el siguiente grupo de coordenadas
    // La prueba pide una coordenada cada 3 a 5 segundos
    await Task.Delay(5000);
}

async Task SendGps(string vehicleId)
{
    // Aproximadamente el 10% de las solicitudes serán inválidas
    // random.Next(0, 10) genera un número entre 0 y 9
    // Si el número es 0, se enviará una solicitud inválida
    bool shouldSendInvalidRequest = random.Next(0, 10) == 0;

    double lat;
    double lng;

    // VH-003 se usa como vehículo detenido.
    if (vehicleId == "VH-003")
    {
        lat = 4.65;
        lng = -74.10;
    }
    else
    {
        // Genera coordenadas aleatorias dentro de un rango aproximado de Bogotá.
        // Latitud: entre 4.60 y 4.75
        // Longitud: entre -74.20 y -73.95
        lat = 4.60 + random.NextDouble() * 0.15;
        lng = -74.20 + random.NextDouble() * 0.25;
    }

    // Se usa object porque los payloads válidos e inválidos pueden tener estructuras diferentes.
    // Por ejemplo, una solicitud inválida puede no incluir el campo timestamp.
    object data;

    if (shouldSendInvalidRequest)
    {
        // Se selecciona aleatoriamente un tipo de error
        // para probar las validaciones del backend
        int errorType = random.Next(0, 3);

        switch (errorType)
        {
            case 0:
                // Solicitud inválida: vehicleId vacío
                data = new
                {
                    vehicleId = "",
                    lat = lat,
                    lng = lng,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                break;

            case 1:
                // Solicitud inválida: latitud fuera del rango permitido
                // La latitud válida debe estar entre -90 y 90
                data = new
                {
                    vehicleId = vehicleId,
                    lat = 999,
                    lng = lng,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                break;

            default:
                // Solicitud inválida: falta el timestamp
                data = new
                {
                    vehicleId = vehicleId,
                    lat = lat,
                    lng = lng
                };
                break;
        }

        Console.WriteLine($"[{vehicleId}] Enviando solicitud INVÁLIDA");
    }
    else
    {
        // Solicitud GPS válida
        // El timestamp se envía en formato ISO 8601 usando ToString("o")
        data = new
        {
            vehicleId = vehicleId,
            lat = lat,
            lng = lng,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        Console.WriteLine($"[{vehicleId}] Enviando solicitud VÁLIDA");
    }

    // Convierte el objeto de C# a formato JSON
    var json = JsonSerializer.Serialize(data);

    // Crea el contenido HTTP indicando que se enviará JSON
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    // Envía la solicitud POST al endpoint /gps del backend
    var response = await httpClient.PostAsync("http://localhost:5149/gps", content);

    // Muestra en consola el código de respuesta del backend
    // Ejemplos esperados:
    // Created = 201 para solicitudes válidas
    // BadRequest = 400 para solicitudes inválidas
}