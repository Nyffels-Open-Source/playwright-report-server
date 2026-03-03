# Playwright Report Server

Open source .NET API for storing and serving Playwright test reports and artifacts.

## Features

- Upload and store Playwright report artifacts
- Serve saved reports and files over HTTP
- SQLite persistence out of the box
- Retention cleanup for old reports
- Container-first deployment support

## Quick Start (Docker Compose)

Use the included [`docker-compose.yml`](docker-compose.yml).
Set the endpoint write token directly in Compose via `ENDPOINT_WRITE_API_KEY`:

```bash
ENDPOINT_WRITE_API_KEY=your-strong-secret docker compose up -d
```

API base URL: `http://localhost:8080`
Scalar API Reference: `http://localhost:8080/scalar`

## Local Development

Prerequisites:

- .NET SDK 10.0+
- Docker (optional)

Run locally:

```bash
dotnet restore src/PlaywrightReportServer.sln
dotnet run --project src/PlaywrightReportServer.Api/PlaywrightReportServer.Api.csproj
```

## Configuration

The default data directory inside the container is `/data` and stores:

- SQLite DB: `/data/db.sqlite`
- Reports: `/data/reports`
- Upload temp files: `/data/uploads`

Use a persistent Docker volume or host mount for `/data`.

## Endpoint Security

Write endpoints are protected with an API key:

- `POST /api/reports`
- `DELETE /api/reports/{id}`

Configure the key using:

```bash
EndpointSecurity__WriteApiKey=your-strong-secret
```

In Docker Compose this is wired through:

```yaml
environment:
  EndpointSecurity__WriteApiKey: ${ENDPOINT_WRITE_API_KEY:-change-me-in-production}
```

Send it in the request header:

```http
X-Api-Key: your-strong-secret
```

`GET` endpoints remain public.

## API Documentation

- Scalar UI: `/scalar`
- OpenAPI JSON: `/openapi/v1.json`

## Repository Structure

- `src/`: solution and .NET projects
- `infra/`: Docker assets

## Contributing

Contributions are welcome. Open an issue for bugs/features or submit a pull request with a clear description of the change.

## License

Licensed under the MIT License. See [`LICENSE`](LICENSE).
