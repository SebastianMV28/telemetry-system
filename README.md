# Fleet Telemetry System

Sistema de monitoreo y telemetría de flotas GPS desarrollado como prueba técnica para el rol de Fullstack Developer.

El proyecto permite recibir coordenadas GPS de vehículos, almacenar su información, calcular su estado actual y visualizar los datos en un dashboard web con tabla y mapa.

---

## Descripción general

Este sistema simula una flota de vehículos enviando coordenadas GPS a un backend mediante un endpoint HTTP.

El backend valida los datos recibidos, almacena la información en memoria y expone endpoints para consultar o eliminar vehículos.

El frontend consume la API y muestra:

- Lista de vehículos monitoreados.
- Estado actual de cada vehículo.
- Última transmisión recibida.
- Latitud y longitud.
- Mapa con la última ubicación de cada vehículo.
- Actualización automática cada 5 segundos.

---

## Tecnologías utilizadas

### Backend

- C#
- ASP.NET Core Web API
- Almacenamiento en memoria usando `Dictionary`

### Simulador

- C#
- `HttpClient`
- `System.Text.Json`

### Frontend

- React
- Vite
- Leaflet
- React Leaflet
- CSS

---

## Estructura del proyecto

```txt
telemetry-system/
  Backend/
    Controllers/
    Models/
    Program.cs

  Simulator/
    Program.cs

  frontend/
    src/
      App.jsx
      App.css
      main.jsx

  README.md
```

---

## Cómo correr el proyecto localmente

> Requisito previo: tener instalado .NET SDK y Node.js.

### 1. Ejecutar el backend

Desde la carpeta del backend:

```bash
cd Backend
dotnet run
```

El backend corre en:

```txt
http://localhost:5149
```

---

### 2. Ejecutar el simulador

En otra terminal, desde la carpeta del simulador:

```bash
cd Simulator
dotnet run
```

El simulador envía coordenadas GPS al endpoint:

```txt
POST http://localhost:5149/gps
```

---

### 3. Ejecutar el frontend

En otra terminal, desde la carpeta del frontend:

```bash
cd frontend
npm install
npm run dev
```

El frontend corre en:

```txt
http://localhost:5173
```

---

## Endpoints principales

### POST `/gps`

Recibe la ubicación GPS de un vehículo.

Ejemplo de request válido:

```json
{
    "vehicleId": "VH-001",
    "lat": 4.711,
    "lng": -74.0721,
    "timestamp": "2026-06-24T10:00:00Z"
}
```

Respuestas esperadas:

- `201 Created`: coordenada almacenada correctamente.
- `400 Bad Request`: datos inválidos o incompletos.

Validaciones implementadas:

- `vehicleId` obligatorio y no vacío.
- `lat` requerida y dentro del rango `-90` a `90`.
- `lng` requerida y dentro del rango `-180` a `180`.
- `timestamp` obligatorio y válido.

---

### GET `/vehicles`

Devuelve el estado actual de todos los vehículos registrados.

Ejemplo de respuesta:

```json
[
    {
        "vehicle_id": "VH-001",
        "last_lat": 4.6855,
        "last_lng": -73.9831,
        "last_seen": "2026-06-24T10:00:00Z",
        "status": "En movimiento"
    }
]
```

Estados posibles:

- `En movimiento`
- `Detenido`
- `Sin señal`

La lógica de estados se basa en la última transmisión recibida y en la comparación entre coordenadas recientes:

- Si el vehículo recibe coordenadas recientes y estas cambian, se considera `En movimiento`.
- Si el vehículo mantiene la misma coordenada, se considera `Detenido`.
- Si no se reciben datos durante más de 2 minutos, se considera `Sin señal`.

---

### GET `/vehicles/{id}`

Consulta un vehículo específico por ID.

Ejemplo:

```txt
GET /vehicles/VH-001
```

Respuestas:

- `200 OK`: vehículo encontrado.
- `404 Not Found`: vehículo no encontrado.

---

### DELETE `/vehicles/{id}`

Elimina un vehículo del sistema.

Ejemplo:

```txt
DELETE /vehicles/VH-001
```

Respuestas:

- `204 No Content`: vehículo eliminado correctamente.
- `404 Not Found`: vehículo no encontrado.

---

## Simulador de telemetría

El simulador fue construido como un script independiente en C#.

Características implementadas:

- Simula 5 vehículos.
- Envía una coordenada por vehículo cada 5 segundos.
- Usa coordenadas dentro de un rango aproximado de Bogotá.
- Mantiene el vehículo `VH-003` en la misma posición para simular un vehículo detenido.
- Inyecta errores en aproximadamente el 10% de los requests.

Ejemplos de errores simulados:

- `vehicleId` vacío.
- Latitud fuera de rango.
- Request sin `timestamp`.

Esto permite probar que el backend responde correctamente con `400 Bad Request` ante datos inválidos.

---

## Frontend

El frontend fue desarrollado con React y Vite.

Funcionalidades implementadas:

- Consume el endpoint `GET /vehicles`.
- Muestra una tabla con:
    - ID del vehículo.
    - Estado.
    - Última transmisión.
    - Latitud.
    - Longitud.
- Usa etiquetas de color para identificar visualmente el estado.
- Actualiza los datos automáticamente cada 5 segundos usando polling.
- Muestra un mapa con Leaflet.
- Ubica cada vehículo con un marcador.
- Cada marcador muestra un popup con el ID y estado del vehículo.

---

## Arquitectura y decisiones técnicas

La solución se dividió en tres partes principales:

### 1. Backend

El backend es responsable de recibir, validar, almacenar y exponer la información GPS.

Se usó almacenamiento en memoria con `Dictionary` para mantener el prototipo simple. Esta decisión permite enfocarse en la lógica de negocio, validaciones y endpoints requeridos sin agregar complejidad innecesaria con una base de datos externa.

### 2. Simulador

El simulador es responsable de generar datos GPS periódicos.

Permite probar el backend sin depender de dispositivos reales. Además, genera requests inválidos para validar el manejo de errores del backend.

### 3. Frontend

El frontend es responsable de visualizar el estado actual de la flota.

Consume la API mediante polling cada 5 segundos y presenta la información en una tabla y en un mapa interactivo.

Elegí esta arquitectura porque separa responsabilidades de forma clara:

- El backend procesa y valida datos.
- El simulador genera eventos GPS.
- El frontend visualiza el estado del sistema.

---

## Reflexión sobre eliminación de vehículos

Si en un sistema real existiera un caché como Redis y una base de datos persistente, al eliminar un vehículo sería necesario garantizar que la información se elimine o invalide en ambos lugares.

Por ejemplo, si se elimina el vehículo en la base de datos pero queda almacenado en Redis, el sistema podría seguir mostrando información antigua. Esto generaría inconsistencias entre lo que realmente existe en la base de datos y lo que ve el usuario en la aplicación.

Para evitar este problema, se debería definir una estrategia clara, como:

- Eliminar primero el registro en la base de datos y luego invalidar el caché.
- Usar transacciones o mecanismos de compensación si una operación falla.
- Asegurar que las lecturas posteriores no sigan usando datos obsoletos.
- Registrar errores y reintentar la eliminación del caché si es necesario.
- Definir tiempos de expiración para evitar que datos viejos permanezcan indefinidamente en caché.

En resumen, lo importante es mantener consistencia entre la fuente persistente y el caché para evitar que el usuario vea vehículos eliminados o información desactualizada.

---

## Reporte de IA

### 1. ¿Qué herramientas de IA usé?

Usé herramientas de IA conversacional como apoyo durante el desarrollo de la prueba.

---

### 2. ¿Para qué tareas específicas me apoyé en la IA?

Me apoyé en IA para:

- Entender mejor los requerimientos de la prueba.
- Organizar el desarrollo por etapas.
- Revisar la lógica del backend.
- Mejorar la inyección de errores del simulador.
- Aclarar el uso de timestamps en formato ISO 8601.
- Revisar problemas de integración entre backend, simulador y frontend.
- Estructurar la documentación del proyecto.
- Preparar una explicación clara para la sustentación.

---

### 3. ¿Qué error de la IA encontré y cómo lo corregí?

Un error importante fue que inicialmente la IA confundió el contexto del simulador y propuso validaciones genéricas en lugar de enfocarse en la inyección de errores requerida por la prueba.

Lo corregí revisando nuevamente el enunciado y ajustando el simulador para que realmente enviara aproximadamente un 10% de requests inválidos o incompletos.

Otro punto que corregí fue mantener consistencia con los nombres usados en mi proyecto, por ejemplo `vehicleId`, `lat`, `lng` y `timestamp`, evitando cambiar innecesariamente toda la estructura del backend.

También validé manualmente que el backend respondiera con los códigos HTTP esperados y que el frontend consumiera correctamente los datos expuestos por la API.

---

## Limitaciones actuales

- El almacenamiento es en memoria, por lo que los datos se pierden al reiniciar el backend.
- No se implementó autenticación.
- No se implementó base de datos persistente.
- No se implementaron WebSockets ni Server-Sent Events; se usó polling cada 5 segundos.
- La interfaz es funcional y simple, priorizando claridad sobre diseño visual avanzado.

Estas decisiones se tomaron para mantener el alcance adecuado al tiempo de la prueba y cumplir primero los requerimientos principales.

---

## Posibles mejoras futuras

Algunas mejoras que podrían implementarse en una siguiente versión son:

- Persistencia en base de datos como PostgreSQL, SQLite o MongoDB.
- Uso de Redis para caché de estados recientes.
- Autenticación y autorización para proteger los endpoints.
- Tests unitarios para la lógica de estados.
- Dockerfile o docker-compose para facilitar la ejecución del sistema.
- Actualización en tiempo real usando WebSockets o Server-Sent Events.
- Mejoras visuales en el dashboard.
- Filtros por estado o por ID de vehículo.

---

## Video de sustentación

Link del video:

```txt
PENDIENTE: agregar aquí el enlace de YouTube no listado
```

En el video se muestra:

- Backend corriendo.
- Simulador enviando datos GPS.
- Requests válidos e inválidos.
- Dashboard actualizándose.
- Tabla de vehículos.
- Mapa con ubicaciones.
- Explicación breve de decisiones técnicas.
