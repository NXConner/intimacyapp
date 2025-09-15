# Release Checklist

## Security & Privacy
- Verify API key and rate limiting are enabled
- Run encryption/decryption tests and rotate keys if needed
- Review CORS origins
- Run security scan and fix high/critical findings

## Data & Compliance
- Confirm data model migrations applied
- Test GDPR endpoints: erase, export
- Validate privacy policy and consent flows

## Functionality
- API endpoints pass integration tests
- Blazor client: health, analytics, model performance, analyze upload
- Windows WPF stub: analytics posting, SignalR status
- Android stub: preferences saved, analytics posting via emulator

## Performance
- Basic load test: 1k RPS target on read endpoints
- DB query times < 100ms for typical queries

## Observability
- Enable structured logging and log rotation
- Verify health endpoints `/health`, `/healthz`

## Documentation
- README up to date (setup, run, platform apps)
- CI green: build + test

## Deployment
- Environment configs set (API key, CORS)
- Backup and rollback plan ready