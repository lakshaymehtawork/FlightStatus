# Technical Specification - Flight Status Lookup

## 1. Purpose
This specification defines the complete technical contract for the Flight Status lookup feature. It is the implementation source of truth for data model, interface contracts, API behavior, and testable business rules.

## 2. Inputs And Traceability

### 2.1 Input Documents
- question.txt
- BRD.md

### 2.2 BR And FR Coverage Matrix
- BR-001, BR-003, BR-004 -> Sections 3, 4, 5, 6
- BR-002, BR-005, BR-006 -> Sections 2, 7
- BR-007 -> Section 7.2
- BR-008 -> Section 6.7, 7.3

- FR-001 -> Sections 3.1, 5.1, 6.1
- FR-002 -> Section 5.1
- FR-003 -> Sections 4.1, 6.3
- FR-004 -> Sections 4.1, 4.3
- FR-005 -> Section 4.3
- FR-006 -> Sections 3.2, 6.2
- FR-007, FR-008, FR-009, FR-010, FR-011 -> Sections 3.2, 6.2
- FR-012, FR-013, FR-014 -> Section 6.3
- FR-015 -> Sections 5.3, 6.1
- FR-016 -> Sections 3.1, 5.2
- FR-017, FR-018 -> Section 3.3
- FR-019, FR-020 -> Section 3.4
- FR-021, FR-022, FR-023, FR-024 -> Sections 3.1, 6.5, 6.6
- FR-025 -> Sections 4.4, 6.4
- FR-026, FR-027, FR-028 -> Section 7

## 3. Data Model

### 3.1 Core Domain Entities

#### 3.1.1 FlightStatusQuery
Represents a lookup request.

| Field | Type | Nullable | Meaning | Notes |
|---|---|---|---|---|
| flightNumber | string | No | Flight code entered by support agent | Required input |
| date | date-only string (yyyy-MM-dd) | No | Flight operation date | Required input |

#### 3.1.2 FlightStatusResult
Represents normalized API response.

| Field | Type | Nullable | Meaning | Notes |
|---|---|---|---|---|
| flightNumber | string | No | Echo of validated request flight number | Always populated |
| date | date-only string (yyyy-MM-dd) | No | Echo of validated request date | Always populated |
| status | UnifiedFlightStatus | No | Unified flight status | Always populated |
| message | string | No | Human-readable result summary | Required for all responses |
| providerUsed | string | Yes | Name of provider chosen for output | Null when no provider responded |
| lastUpdatedUtc | datetime (ISO 8601 UTC) | Yes | Chosen provider freshness timestamp | Null when no provider responded |
| scheduledDepartureUtc | datetime (ISO 8601 UTC) | Yes | Normalized scheduled departure time | Null if unavailable |
| scheduledArrivalUtc | datetime (ISO 8601 UTC) | Yes | Normalized scheduled arrival time | Null if unavailable |
| actualDepartureUtc | datetime (ISO 8601 UTC) | Yes | Actual departure time | AeroTrack conditional |
| actualArrivalUtc | datetime (ISO 8601 UTC) | Yes | Actual arrival time | AeroTrack conditional |
| terminal | string | Yes | Departure terminal | AeroTrack conditional |
| gate | string | Yes | Departure gate | AeroTrack conditional |
| delayReason | string | Yes | Delay reason text | Only when status is Delayed and source provides reason |

#### 3.1.3 ProviderFlightStatusCandidate
Intermediate normalized candidate returned by each provider implementation.

| Field | Type | Nullable | Meaning | Notes |
|---|---|---|---|---|
| providerName | string | No | Logical provider identifier | Expected values: AeroTrack, QuickFlight |
| flightNumber | string | No | Provider-correlated flight number | Required |
| date | date-only string (yyyy-MM-dd) | No | Provider-correlated date | Required |
| status | UnifiedFlightStatus | No | Provider-normalized status | Required |
| lastUpdatedUtc | datetime (ISO 8601 UTC) | No | Provider update timestamp for selection | Required |
| scheduledDepartureUtc | datetime (ISO 8601 UTC) | Yes | Scheduled departure time | Null if unavailable |
| scheduledArrivalUtc | datetime (ISO 8601 UTC) | Yes | Scheduled arrival time | Null if unavailable |
| actualDepartureUtc | datetime (ISO 8601 UTC) | Yes | Actual departure time | AeroTrack conditional |
| actualArrivalUtc | datetime (ISO 8601 UTC) | Yes | Actual arrival time | AeroTrack conditional |
| terminal | string | Yes | Terminal | AeroTrack conditional |
| gate | string | Yes | Gate | AeroTrack conditional |
| delayReason | string | Yes | Delay reason | Delayed conditional |
| statusRaw | string | Yes | Original provider status value | For traceability and logs |

#### 3.1.4 ApiError
Standard error payload for 4xx and 5xx responses.

| Field | Type | Nullable | Meaning | Notes |
|---|---|---|---|---|
| code | string | No | Stable machine-readable error code | Example: VALIDATION_ERROR |
| message | string | No | Human-readable summary | Required |
| details | array of string | Yes | Field-level or context details | Empty or null when not applicable |
| requestId | string | Yes | Correlation identifier for diagnostics | Optional |

### 3.2 Unified Enum Definitions

#### 3.2.1 UnifiedFlightStatus

| Enum Value | Meaning |
|---|---|
| OnTime | Departing or arrived within 15 minutes of schedule |
| Delayed | Departure or arrival pushed beyond 15 minutes |
| Cancelled | Flight will not operate |
| Diverted | Flight landed at a different airport |
| Unknown | Provider returned no usable status |

### 3.3 Provider Raw Schema Contracts

#### 3.3.1 AeroTrackRawResponse

| Field | Type | Nullable | Meaning |
|---|---|---|---|
| flight_status | string | No | AeroTrack status vocabulary value |
| sched_dep_utc | datetime | Yes | Scheduled departure |
| sched_arr_utc | datetime | Yes | Scheduled arrival |
| actual_dep_utc | datetime | Yes | Actual departure |
| actual_arr_utc | datetime | Yes | Actual arrival |
| terminal_code | string | Yes | Terminal |
| gate_code | string | Yes | Gate |
| delay_reason_text | string | Yes | Delay reason when delayed |
| last_updated_utc | datetime | No | Source freshness timestamp |

#### 3.3.2 QuickFlightRawResponse

| Field | Type | Nullable | Meaning |
|---|---|---|---|
| status | string | No | QuickFlight status vocabulary value |
| scheduledDepartureUtc | datetime | Yes | Scheduled departure |
| scheduledArrivalUtc | datetime | Yes | Scheduled arrival |
| lastUpdatedUtc | datetime | No | Source freshness timestamp |

### 3.4 Provider Status Vocabularies And Mapping Inputs
These values define deterministic stub vocabularies for testability.

#### 3.4.1 AeroTrackStatusRaw
- ON_SCHEDULE
- DELAYED
- CANCELED
- DIVERTED
- UNKNOWN

#### 3.4.2 QuickFlightStatusRaw
- ON_TIME
- LATE
- CANCELLED
- DIVERTED
- UNKNOWN

## 4. Interfaces And Dependency Contracts

### 4.1 Provider Abstraction

```csharp
public interface IFlightStatusProvider
{
		string ProviderName { get; }

		Task<ProviderFlightStatusCandidate?> GetStatusAsync(
				FlightStatusQuery query,
				CancellationToken cancellationToken);
}
```

Semantics:
- Returns null when provider has no deterministic stub match for the query.
- Throws only for exceptional provider failure conditions (to be caught by aggregation layer).

### 4.2 Aggregation Contract

```csharp
public interface IFlightStatusService
{
		Task<FlightStatusResult> GetFlightStatusAsync(
				FlightStatusQuery query,
				CancellationToken cancellationToken);
}
```

Semantics:
- Coordinates all registered providers.
- Applies selection and fallback rules.
- Returns a business result model, including Unknown when no providers respond.

### 4.3 DI Composition Contract
Required registrations:
- Register both provider implementations against IFlightStatusProvider.
- Register one service implementation for IFlightStatusService.
- Endpoint handler depends on IFlightStatusService only, never on concrete providers.

Planned DI-backed implementations:
- AeroTrackStubProvider : IFlightStatusProvider
- QuickFlightStubProvider : IFlightStatusProvider
- FlightStatusService : IFlightStatusService

### 4.4 Stub Determinism Contract
- Each provider implementation must use a fixed, hardcoded response dataset.
- The same input query must always produce the same output (or null) across runs.
- Inputs not present in hardcoded dataset must return null.

## 5. HTTP API Contract

### 5.1 Endpoint Definition
- Method: GET
- Path: /flights/status
- Query parameters:
	- flightNumber: string, required
	- date: string, required, format yyyy-MM-dd

### 5.2 Success Response
- HTTP 200 OK
- Content-Type: application/json
- Body: FlightStatusResult

Example success (provider selected):

```json
{
	"flightNumber": "SR101",
	"date": "2026-07-20",
	"status": "Delayed",
	"message": "Status provided by AeroTrack",
	"providerUsed": "AeroTrack",
	"lastUpdatedUtc": "2026-07-20T08:15:00Z",
	"scheduledDepartureUtc": "2026-07-20T07:30:00Z",
	"scheduledArrivalUtc": "2026-07-20T09:45:00Z",
	"actualDepartureUtc": "2026-07-20T07:55:00Z",
	"actualArrivalUtc": null,
	"terminal": "T2",
	"gate": "A12",
	"delayReason": "Crew rotation delay"
}
```

Example success (neither provider responds):

```json
{
	"flightNumber": "SR404",
	"date": "2026-07-20",
	"status": "Unknown",
	"message": "No provider returned usable status for the requested flight and date.",
	"providerUsed": null,
	"lastUpdatedUtc": null,
	"scheduledDepartureUtc": null,
	"scheduledArrivalUtc": null,
	"actualDepartureUtc": null,
	"actualArrivalUtc": null,
	"terminal": null,
	"gate": null,
	"delayReason": null
}
```

### 5.3 Error Responses

#### 5.3.1 Missing Required Inputs
- HTTP 400 Bad Request
- Error payload: ApiError

Example:

```json
{
	"code": "VALIDATION_ERROR",
	"message": "Request validation failed.",
	"details": [
		"flightNumber is required.",
		"date is required."
	],
	"requestId": "00-8a5d..."
}
```

#### 5.3.2 Invalid Date Format
- HTTP 400 Bad Request
- Error payload: ApiError

Example:

```json
{
	"code": "VALIDATION_ERROR",
	"message": "Request validation failed.",
	"details": [
		"date must be in yyyy-MM-dd format."
	],
	"requestId": "00-8a5d..."
}
```

#### 5.3.3 Unexpected Unhandled Failure
- HTTP 500 Internal Server Error
- Error payload: ApiError

### 5.4 Serialization Conventions
- JSON property naming: camelCase.
- Date-only values serialized as yyyy-MM-dd strings.
- DateTime values serialized as ISO 8601 UTC timestamps with Z suffix.
- Enum values serialized as string literals exactly matching declared enum names.

## 6. Business Rules (Testable)

### 6.1 Validation Rules
- BR-RULE-001: Request is invalid when flightNumber is null, empty, or whitespace; API returns 400 with VALIDATION_ERROR.
- BR-RULE-002: Request is invalid when date is missing; API returns 400 with VALIDATION_ERROR.
- BR-RULE-003: Request is invalid when date does not match yyyy-MM-dd; API returns 400 with VALIDATION_ERROR.

### 6.2 Status Normalization Rules

#### 6.2.1 AeroTrack Raw To Unified
- BR-RULE-004: AeroTrack ON_SCHEDULE maps to OnTime.
- BR-RULE-005: AeroTrack DELAYED maps to Delayed.
- BR-RULE-006: AeroTrack CANCELED maps to Cancelled.
- BR-RULE-007: AeroTrack DIVERTED maps to Diverted.
- BR-RULE-008: AeroTrack UNKNOWN maps to Unknown.
- BR-RULE-009: Any unrecognized AeroTrack status maps to Unknown.

#### 6.2.2 QuickFlight Raw To Unified
- BR-RULE-010: QuickFlight ON_TIME maps to OnTime.
- BR-RULE-011: QuickFlight LATE maps to Delayed.
- BR-RULE-012: QuickFlight CANCELLED maps to Cancelled.
- BR-RULE-013: QuickFlight DIVERTED maps to Diverted.
- BR-RULE-014: QuickFlight UNKNOWN maps to Unknown.
- BR-RULE-015: Any unrecognized QuickFlight status maps to Unknown.

#### 6.2.3 Unified Meaning Rules
- BR-RULE-016: A result is OnTime when departure or arrival is within 15 minutes of schedule according to mapped provider status.
- BR-RULE-017: A result is Delayed when departure or arrival exceeds 15 minutes from schedule according to mapped provider status.
- BR-RULE-018: Cancelled means flight will not operate.
- BR-RULE-019: Diverted means flight landed at a different airport.
- BR-RULE-020: Unknown means provider returned no usable status.

### 6.3 Provider Selection And Fallback Rules
- BR-RULE-021: Backend must attempt both providers for each valid request.
- BR-RULE-022: If both providers return non-null candidates, select the candidate with later lastUpdatedUtc.
- BR-RULE-023: If only one provider returns non-null candidate, select that candidate.
- BR-RULE-024: If neither provider returns a non-null candidate, return HTTP 200 with status Unknown and appropriate message.
- BR-RULE-025: If both providers return same lastUpdatedUtc, select AeroTrack candidate for deterministic tie-breaking.

### 6.4 Provider Fault Tolerance Rules
- BR-RULE-026: Provider exception from one provider must not fail processing of the other provider.
- BR-RULE-027: If one provider throws exception and the other returns candidate, return selected successful candidate.
- BR-RULE-028: If all providers throw exception or return null, return Unknown business response per BR-RULE-024.
- BR-RULE-029: Provider stubs must be deterministic and input-repeatable.

### 6.5 Field Population Rules
- BR-RULE-030: gate, terminal, delayReason are populated only when chosen provider supplies those values.
- BR-RULE-031: QuickFlight-origin response must keep gate, terminal, delayReason as null.
- BR-RULE-032: delayReason is null unless status is Delayed and source provided reason text.
- BR-RULE-033: message must always be non-empty.

### 6.6 Frontend Display Contract Rules
- BR-RULE-034: UI color map must be OnTime green, Delayed amber, Cancelled red, Diverted red, Unknown grey.
- BR-RULE-035: UI must hide AeroTrack-only fields when null.
- BR-RULE-036: UI must show an error state for non-2xx API responses.

### 6.7 Security And Compliance Rules
- BR-RULE-037: No real flight APIs or live credentials may be used.
- BR-RULE-038: No real secrets may be committed.

## 7. Delivery And Documentation Contracts

### 7.1 Required Artifact Presence
- README.md present and contains setup and run guidance.
- BRD.md present.
- spec.md present and authored before implementation files.
- prompts.md present and updated with significant prompts.
- reflection.md present.

### 7.2 AI Usage Documentation
- prompts.md entries must include accepted and rejected or modified prompt outcomes.
- README.md must include Copilot usage summary.

### 7.3 No-Secret Rule
- No API keys, tokens, or real credentials in source or docs.

## 8. Assumptions Used In This Spec
- SPEC-ASSUMPTION-001: Equal lastUpdatedUtc tie-break uses AeroTrack precedence for deterministic behavior.
- SPEC-ASSUMPTION-002: Unknown business outcome when neither provider responds is returned as HTTP 200 with normalized body.
- SPEC-ASSUMPTION-003: Raw provider vocabularies listed in Section 3.4 are fixed stub vocabularies for this assignment.

## 9. Resolved Contract Decisions
The following contract decisions are finalized using question.txt as the authoritative source plus explicit deterministic assumptions for unspecified edge cases:
1. Equal lastUpdatedUtc tie-break uses AeroTrack precedence.
2. Neither provider responding returns HTTP 200 with status Unknown and an appropriate message.
3. Section 3.4 raw vocabulary names are fixed stub contract values for this assignment.


