# Intimacy AI (MVP)

A minimal end-to-end scaffold based on the provided requirements PDF:
- .NET 8 Web API (SQLite + EF Core) with API key middleware
- Blazor WebAssembly client with configurable API base URL and key

## Prerequisites
- Linux/macOS/Windows
- .NET 8 SDK (this repo uses local install via script if needed)

## Getting Started

### 1) Backend API
```
export PATH="$PWD/.dotnet:$PATH"; export DOTNET_ROOT="$PWD/.dotnet"
cd intimacy-ai/src/Server
# First run creates/updates SQLite DB
dotnet run -c Release --urls http://0.0.0.0:5087
```
- Swagger: http://localhost:5087/swagger
- Health: http://localhost:5087/health

API security: send header `X-API-Key: dev-key` (change in `appsettings.json` -> `Security:ApiKey`).

### 2) Frontend Client (Blazor WASM)
```
export PATH="$PWD/.dotnet:$PATH"; export DOTNET_ROOT="$PWD/.dotnet"
cd intimacy-ai/src/Client
dotnet run -c Release --urls http://0.0.0.0:5175
```
Open http://localhost:5175

- Go to Settings to set API Base URL (default http://localhost:5087) and API Key.
- Home page loads health, recent analytics, and allows posting a sample analytics record.

## Development
- Solution file: `intimacy-ai/IntimacyAI.sln`
- Projects:
  - `src/Server` - ASP.NET Core minimal API, EF Core SQLite
  - `src/Client` - Blazor WebAssembly

### Database
- SQLite file: `src/Server/app.db`
- EF Core migrations applied on startup.

### Environment variables
- None required. Configuration via `appsettings.json` and client local storage.

## CI
A simple GitHub Actions workflow builds the solution on pushes/PRs.

## License
MIT (replace as needed).