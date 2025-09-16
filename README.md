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

Auth options (API key middleware):
- Preferred: `Authorization: Bearer <API_KEY>`
- Also accepted: header `X-API-Key: <API_KEY>`
- WebSockets/SignalR: query `access_token=<API_KEY>`
- Unprotected endpoints: `/swagger/*`, `/health`, `/healthz`

API security:
- In Development, a default key is set in `src/Server/appsettings.Development.json` (`Security:ApiKey`).
- In Production, set environment variables and do not store secrets in files:
  - `Security__ApiKey` = your-strong-key
  - `Security__EncryptionKey` = Base64-encoded 32-byte key for AES-GCM
  - Optional CORS override: `CORS_ALLOWED_ORIGINS` = `https://app.example.com,https://admin.example.com`

### 2) Frontend Client (Blazor WASM)
```
export PATH="$PWD/.dotnet:$PATH"; export DOTNET_ROOT="$PWD/.dotnet"
cd intimacy-ai/src/Client
dotnet run -c Release --urls http://0.0.0.0:5175
```
Open http://localhost:5175

- Go to Settings to set API Base URL (default http://localhost:5087) and API Key.
- Home page loads health, recent analytics, and allows posting a sample analytics record.

SignalR configuration:
- The Analyze page connects to `$(ApiBaseUrl)/hubs/analysis`.
- The API key is passed as an access token for WebSocket auth.

### Helper Scripts
- Unix: `./run.sh server|client|build|test`
- Windows: `run.cmd server|client|build|test`

## Development
- Solution file: `intimacy-ai/IntimacyAI.sln`
- Projects:
  - `src/Server` - ASP.NET Core minimal API, EF Core SQLite
  - `src/Client` - Blazor WebAssembly
  - `platform/windows/WpfApp` - Windows WPF stub app (not in solution)
  - `platform/android` - Android Kotlin/Compose stub app (standalone Gradle)

### Database
- SQLite file is created at runtime (`src/Server/app.db`) and is ignored by git.
- EF Core migrations are applied automatically on startup.

### Environment variables
- Development: provided in `appsettings.Development.json`.
- Production: provide via env vars. Example (bash):
```
export Security__ApiKey="<your-key>"
export Security__EncryptionKey="<base64-32-bytes>"
export CORS_ALLOWED_ORIGINS="https://app.example.com"
```
Note: if `Security:ApiKey` is not configured in Production, the API returns `503 API key not configured`.

## CI
A simple GitHub Actions workflow builds the solution on pushes/PRs.

## Windows WPF App
Requires Windows with .NET 8 SDK and Windows 10 SDK.

From `platform/windows/WpfApp`:
```
dotnet restore
dotnet build -c Release
# Run from Windows shell:
dotnet run -c Release
```
App calls API at `http://localhost:5087` and uses `X-API-Key: dev-key`.

## Android App (Stub)
Requires Android Studio / Gradle.

From `platform/android` in Android environment:
- Open the folder in Android Studio and let it generate Gradle wrapper and sync.
- Use an emulator. The app posts to `http://10.0.2.2:5087` with `X-API-Key: dev-key`.

### Emulator/Device Setup
- Android Emulator: ensure host API is reachable at `10.0.2.2:5087` and CORS allows emulator origin.
- Physical Android device: connect to host IP on LAN, update Base URL in the app UI accordingly.
- Windows: if API runs on WSL2, ensure port forwarding to Windows host.

## Release Checklist
See `RELEASE_CHECKLIST.md`.
