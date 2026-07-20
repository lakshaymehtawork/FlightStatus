
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
| npm | 10.x+ | `npm --version` | https://nodejs.org/en/download |
| Angular CLI | 22.x | `npx ng version` | https://angular.dev/tools/cli |
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

The frontend can use the locally installed CLI via `npx` after `npm install`.

```powershell
npm install -g @angular/cli@latest  # optional
npx ng version
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
- Backend allows `http://localhost:4200` and `https://flight-status-sand.vercel.app` via `FlightStatus.Api/appsettings.json`

### API Base URL in Frontend
The frontend service reads the backend base URL from environment files.

- `flight-status-ui/src/environments/environment.ts`
  - `production: true`
  - `apiBaseUrl: 'https://flightstatus-orvg.onrender.com'`
- `flight-status-ui/src/environments/environment.development.ts`
  - inherits from `flight-status-ui/src/environments/environment.base.ts`
  - `apiBaseUrl: 'http://localhost:5184'`

Local frontend development uses `http://localhost:5184` for the backend.
To change the backend endpoint for local development, update `apiBaseUrl` in `environment.base.ts`.

---

## 7. Manual Smoke Test

Use UI at `http://localhost:4200` or run API calls directly.

### API Endpoint

`GET /flights/status?flightNumber={code}&date={DD-MM-YYYY}`

API sample requests:

PowerShell example:

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:5184/flights/status?flightNumber=SR100&date=20-07-2026" | ConvertTo-Json -Depth 6
```

curl example:

```bash
curl "http://localhost:5184/flights/status?flightNumber=SR100&date=20-07-2026"
```

### Frontend sample request

1. Start the frontend with `npm start` from `flight-status-ui`.
2. Open `http://localhost:4200` in your browser.
3. Enter `SR100` for Flight Number and `20-07-2026` for Date.
4. Submit the search and confirm the status card appears.

### Smoke Matrix

| Scenario | Input (`flightNumber`, `date`) | Expected Unified Status | Notes |
|---|---|---|---|
| OnTime | `SR100`, `20-07-2026` | `OnTime` | Both providers respond, AeroTrack selected by later timestamp |
| Delayed | `SR200`, `20-07-2026` | `Delayed` | AeroTrack-only response, delay reason present |
| Cancelled | `SR300`, `20-07-2026` | `Cancelled` | QuickFlight-only response |
| Diverted | `SR400`, `20-07-2026` | `Diverted` | Both respond; tie-break resolves to AeroTrack |
| Unknown / not found | `SR500`, `20-07-2026` | `Unknown` | Neither provider returns data |
| Partial-failure style (AeroTrack only) | `SR200`, `20-07-2026` | `Delayed` | QuickFlight absent |
| Partial-failure style (QuickFlight only) | `SR800`, `20-07-2026` | `OnTime` | AeroTrack absent |

Note: Stubs are deterministic and keyed by flight number. Date is accepted and validated but not used for stub lookup.

---

## 8. Assumptions (From BRD)

- ASSUMPTION-001: Date input uses `DD-MM-YYYY`, interpreted as local calendar date for stub simulation.
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

