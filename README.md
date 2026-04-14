# Temperature Telemetry API (.NET)

A lightweight ASP.NET Core Web API for ingesting temperature telemetry from industrial devices.

This project is designed to reflect real-world industrial scenarios where devices send sensor data to a backend system (e.g. SCADA, IIoT platform, monitoring service).

---

## Why I built this

I built this API as part of an industrial software simulation:

- To represent a **device → backend communication layer**
- To integrate with a **.NET Worker Service (Industrial Gateway)**
- To practice **clean API design, DTOs, and HTTP communication**

This API acts as the **receiver of telemetry data** in an IIoT pipeline.

---

## Industrial Context

This API simulates a real-world scenario where:

- PLCs or edge devices send telemetry data
- A gateway aggregates and forwards data
- Backend systems process and visualize data

Typical use cases:
- Machine condition monitoring
- Temperature tracking in production lines
- Predictive maintenance systems

---

## Tech Stack

- .NET 8 / .NET 10 (depending on your setup)
- ASP.NET Core Web API
- Swagger (OpenAPI)
- C#

---

## Architecture

```text
Device Simulator
    ↓
Industrial Gateway (.NET Worker)
    ↓
Temperature API
    ↓
(Future) Database / Dashboard
```

---

## How to Run

### 1. Clone the repository

```bash
git clone https://github.com/Ham15-art/temperature-telemetry-api-dotnet.git
```
### 2. Run the API

```bash
dotnet run
```

---

## Access the API

once running, open Swagger UI: http://localhost:5244/swagger 
(Port will be shown in the console)

---

## API Endpoints

### Endpoint

POST /temperature

### Send Temperature Data

example request:
```json
{
  "deviceId": "device-123",
  "value": 23.5,
  "timestampUtc": "2026-04-09T12:00:00Z"
}
```
example response:
```json
{
  "status": "received",
  "message": "Temperature reading is accepted",
  "data": {
    "deviceId": "device-123",
    "value": 23.5,
    "timestampUtc": "2026-04-09T12:00:00Z"
  }
}
```
> Note: JSON uses camelCase naming, while C# models use PascalCase.
> ASP.NET Core automatically maps between them.

---

## Testing the API

### Option 1: SWAGGER UI

- Open Swagger UI
- Try the POST endpoint directly

### Option 2: curl

```bash
curl -X POST http://localhost:5244/temperature \
  -H "Content-Type: application/json" \
  -d '{"deviceId":"sensor-123","value":25, "timestampUtc": "2026-04-11T13:22:51.996Z"}'
```
---

## Validation Rules

The API validates incoming telemetry data:

- `deviceId` must not be empty
- `value` must be within realistic bounds (-50 to 150 °C)
- `timestampUtc` must be a valid UTC timestamp

Invalid requests return HTTP 400 with a descriptive message.

## Responses

- `200 OK` → Data received successfully
- `400 Bad Request` → Invalid input data

## Example Log Output

### Successful request

```text
info: TemperatureApi.Controllers.TelemetryController[0]
      Request received
info: TemperatureApi.Controllers.TelemetryController[0]
      DeviceId: sensor-123
info: TemperatureApi.Controllers.TelemetryController[0]
      Value: 25
info: TemperatureApi.Controllers.TelemetryController[0]
      TimeStamp: 2026-04-11T13:22:51.9960000Z
```
### Unsuccessful request

```text
info: TemperatureApi.Controllers.TelemetryController[0]
      Request Received
info: TemperatureApi.Controllers.TelemetryController[0]
      DeviceId:
info: TemperatureApi.Controllers.TelemetryController[0]
      Temperature value: 40
info: TemperatureApi.Controllers.TelemetryController[0]
      Timestamp: 04/11/2026 13:22:51
warn: TemperatureApi.Controllers.TelemetryController[0]
      Validation failed: DeviceId missing
```
---

## Integration Example

This API is designed to integrate with the Industrial Gateway (.NET Worker Service):

Flow:
Device Simulator → Worker Service → Temperature API → (Future: Database / Dashboard)

The Worker Service periodically reads simulated sensor data and sends it via HTTP POST to this API.

---

## What this project demonstrates:

- RESTful API design in ASP.NET Core
- DTO-based data contracts (TemperatureReading)
- Integration with distributed systems (Worker Service)
- Simulation of industrial telemetry pipelines (IIoT)
- Clean and extensible backend architecture

---

## Possible Improvements:

- Add persistence (database)
- Add validation (FluentValidation)
- Add logging (Serilog)
- Add authentication
- Add retry handling on client side

---

## Author

Hamza Maach
Industrial Software Developer
Focus: Automation, IIoT, .NET, SCADA systems, HMI