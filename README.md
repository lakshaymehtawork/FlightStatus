
# Flight Status Lookup - SkyRoute

This repository contains a full-stack Flight Status lookup solution:
- Backend: .NET 10 Minimal API
- Tests: xUnit + Moq
- Frontend: Angular 22 (standalone components)

The application accepts `flightNumber` and `date`, queries two deterministic stub providers (AeroTrack and QuickFlight), normalizes responses into a unified status, and displays the result in the UI.

---

## 1. Prerequisites

| Tool | Minimum Version | Check Command | Install Link |
|---|---:|---|---|
| .NET SDK | 10.x | `dotnet --version` | https://dotnet.microsoft.com/en-us/download/dotnet/10.0 |
| Node.js | 20.x | `node --version` | https://nodejs.org/en/download |
| npm | 10.x | `npm --version` | https://nodejs.org/en/download |
| Angular CLI | 22.x | `ng version` | https://angular.dev/tools/cli |
| Git | 2.x+ | `git --version` | https://git-scm.com/downloads |

### Exact Setup Instructions (Clean Machine)

#### A. Install .NET 10 SDK (Windows)
1. Open https://dotnet.microsoft.com/en-us/download/dotnet/10.0
2. Download the latest `.NET SDK 10` for Windows x64.
3. Run installer with default options.
4. Open a new terminal and verify:

```powershell
dotnet --version
```

Version must start with `10.`

#### B. Install Node.js and npm
1. Open https://nodejs.org/en/download
2. Install LTS release (Node 20 or newer).
3. Open a new terminal and verify:

```powershell
node --version
npm --version
```

#### C. Install Angular CLI

```powershell
npm install -g @angular/cli@latest
ng version
```

Confirm Angular CLI major version is 22.x (or compatible with project dependencies).

---

## 2. Clone and Open

```powershell
git clone https://github.com/lakshaymehtawork/FlightStatus.git
cd FlightStatus/submissions/Lakshay/UseCase
```

---

## 3. Backend - Restore, Build, Run

Run from repository root `submissions/Lakshay/UseCase`:

```powershell
dotnet restore FlightStatus.slnx
dotnet build FlightStatus.slnx
dotnet run --project FlightStatus.Api
```

Expected startup log:
- `Now listening on: http://localhost:5184`

---

## 4. Tests - Run Full Suite

Run from repository root `submissions/Lakshay/UseCase`:

```powershell
dotnet test FlightStatus.slnx
```

Expected result:
- All tests pass
- Current baseline in this repository: backend tests are green (`26 passed`)

---

## 5. Frontend - Install and Run

Open a second terminal and run:

```powershell
cd flight-status-ui
npm install
npm start
```

This starts Angular dev server at:
- `http://localhost:4200`

Optional checks:

```powershell
npx ng test --watch=false
npx ng build --configuration production
```

Expected result:
- Frontend unit tests pass
- Production build succeeds

---

## 6. Configuration and Environment

### Default Ports
- Backend HTTP: `http://localhost:5184`
- Backend HTTPS: `https://localhost:7154` (dev profile also exposes HTTP)
- Frontend dev server: `http://localhost:4200`

### Required Environment Variables
- None required for local run
- No credentials, tokens, or secrets are required

### CORS
- Backend allows `http://localhost:4200` via `FlightStatus.Api/appsettings.json`

### API Base URL in Frontend
The frontend service reads base URL from environment files:
- `flight-status-ui/src/environments/environment.ts`
- `flight-status-ui/src/environments/environment.development.ts`

Current value:
- `http://localhost:5184`

To change backend endpoint, update `apiBaseUrl` in both files above.

---

## 7. Manual Smoke Test

Use UI at `http://localhost:4200` or run API calls directly.

### API Endpoint

`GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}`

PowerShell example:

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:5184/flights/status?flightNumber=SR100&date=2026-07-20" | ConvertTo-Json -Depth 6
```

### Smoke Matrix

| Scenario | Input (`flightNumber`, `date`) | Expected Unified Status | Notes |
|---|---|---|---|
| OnTime | `SR100`, `2026-07-20` | `OnTime` | Both providers respond, AeroTrack selected by later timestamp |
| Delayed | `SR200`, `2026-07-20` | `Delayed` | AeroTrack-only response, delay reason present |
| Cancelled | `SR300`, `2026-07-20` | `Cancelled` | QuickFlight-only response |
| Diverted | `SR400`, `2026-07-20` | `Diverted` | Both respond; tie-break resolves to AeroTrack |
| Unknown / not found | `SR500`, `2026-07-20` | `Unknown` | Neither provider returns data |
| Partial-failure style (AeroTrack only) | `SR200`, `2026-07-20` | `Delayed` | QuickFlight absent |
| Partial-failure style (QuickFlight only) | `SR800`, `2026-07-20` | `OnTime` | AeroTrack absent |

Note: Stubs are deterministic and keyed by flight number. Date is accepted and validated but not used for stub lookup.

---

## 8. Assumptions (From BRD)

- ASSUMPTION-001: Date input uses `yyyy-MM-dd`, interpreted as local calendar date for stub simulation.
- ASSUMPTION-002: Flight number validation is minimal (non-empty), no strict IATA/ICAO regex.
- ASSUMPTION-003: Missing means null/empty/whitespace for `flightNumber`, null/empty for `date`.
- ASSUMPTION-004: Invalid date format returns HTTP 400 with descriptive payload.
- ASSUMPTION-005: Provider calls execute concurrently.
- ASSUMPTION-006: Provider failures are isolated; one failing provider does not fail whole request if another succeeds.
- ASSUMPTION-007: Provider timeout default target is 2 seconds for resilience simulation.
- ASSUMPTION-008: Equal `lastUpdatedUtc` tie-break defaults to AeroTrack.
- ASSUMPTION-009: Unknown response includes human-readable message for support users.
- ASSUMPTION-010: Neither-provider-data returns HTTP 200 with business status `Unknown`.
- ASSUMPTION-011: Successful normalization from at least one provider returns HTTP 200.
- ASSUMPTION-012: CORS allows local frontend dev origin (`http://localhost:4200`) and is configurable.
- ASSUMPTION-013: No authentication/authorization required for this assignment.
- ASSUMPTION-014: Logging minimum is INFO lifecycle + WARN provider failures.
- ASSUMPTION-015: Datetime serialization uses ISO 8601 UTC.
- ASSUMPTION-016: No persistence/database; data comes from deterministic in-memory stubs.

---

## 9. Clean-Clone Verification Checklist

1. Install prerequisites and verify versions.
2. Clone repository and move to `submissions/Lakshay/UseCase`.
3. Run backend restore/build/run commands.
4. In second terminal run frontend install/start commands.
5. Open `http://localhost:4200`.
6. Search `SR100` with date `2026-07-20`.
7. Confirm status card appears with `OnTime` and provider details.
8. Run `dotnet test FlightStatus.slnx` and confirm all tests pass.
9. Optionally run frontend tests/build.

This confirms the project is runnable from a clean clone with no hidden setup steps.

