# Coding Standards - Flight Status Use Case

This document defines mandatory coding standards for this project.
It is aligned to evaluator expectations in Question.txt and applies to backend, tests, and frontend work.

## 1. Core Principles

- Follow SOLID, DRY, KISS, and YAGNI.
- Prefer readability and maintainability over clever code.
- Keep classes focused on one responsibility.
- Depend on abstractions, not concrete implementations.

## 2. Naming And Contract Fidelity

Use assignment and spec names exactly where defined.
Do not rename contract types or endpoint paths without updating spec.md first.

Mandatory names and contracts:
- Endpoint path: `GET /flights/status?flightNumber={code}&date={DD-MM-YYYY}`
- Interfaces: `IFlightStatusProvider`, `IFlightStatusService`
- Provider implementations: `AeroTrackStubProvider`, `QuickFlightStubProvider`
- Response model: `FlightStatusResult`
- Unified enum values: `OnTime`, `Delayed`, `Cancelled`, `Diverted`, `Unknown`

General naming rules:
- Use meaningful names (`flightStatusService`, not `svc`).
- Use PascalCase for types and methods, camelCase for locals/parameters.
- Boolean names should read as predicates (`isValid`, `hasErrors`).

API and response contract rules:
- Keep the endpoint signature exactly as specified (query names and date format).
- Keep JSON property naming camelCase and enum serialization as strings.
- Preserve status code contract: validation errors -> 400, unexpected server failures -> 500, no-provider-data business outcome -> 200 with `Unknown`.
- Preserve provider selection contract: newer `lastUpdatedUtc` wins; equal timestamp uses deterministic tie-break.

UI contract fidelity rules:
- Keep status color mapping exact: OnTime green, Delayed amber, Cancelled red, Diverted red, Unknown grey.
- Show AeroTrack-only fields only when present; hide when null/absent.
- Display a clear error state for non-2xx API responses.

## 2.1 Version Control Gate (Evaluator Critical)

- `spec.md` must be committed before any implementation files.
- Never rewrite history in a way that invalidates the spec-first timestamp order.
- Use focused commits with meaningful messages that map to SDLC phases.

## 3. Async And Concurrency Standards

- Use `async`/`await` for all I/O or provider call paths.
- Do not block async code (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- Provider calls must run concurrently where allowed (e.g., `Task.WhenAll`).
- Each provider call must be failure-isolated with its own try/catch.
- One provider failure must not fail the whole request when another provider succeeds.

### Locking And Shared State

- Avoid shared mutable state when possible.
- Stub datasets should be immutable/read-only.
- Do not use `lock` around async flows.
- If async coordination is required in future, use `SemaphoreSlim` with `await`.

## 4. Validation And Error Handling

- Validate request input at the HTTP boundary, not inside business service.
- Missing required fields return `400` with descriptive error payload.
- Invalid formats (for example date parse failure) return `400`.
- Unexpected failures return `500` with safe, non-sensitive error payload.
- Never leak stack traces or secrets to clients.

## 5. Logging Standards

Use structured logging with framework logger.

Required log intent:
- INFO: request received with input fields.
- INFO: each provider result (`providerName`, status or `null`).
- WARN: provider exception (`providerName`, exception type, message, request context).
- INFO: final aggregated result (winner and unified status).

Rules:
- Prefer named log placeholders over string concatenation.
- Do not log secrets, credentials, or sensitive tokens.

## 6. Design And Architecture Rules

- Keep dependency direction inward: API -> Application -> Domain; Infrastructure -> Domain.
- Endpoint/controller must depend on `IFlightStatusService` only.
- Service must consume `IEnumerable<IFlightStatusProvider>`.
- Providers act as adapters from provider schema to normalized candidate model.
- Keep mapping logic in provider adapters, not in endpoint.

## 7. Testing Expectations (Evaluator-Facing)

- Every BR-RULE in spec.md must have at least one test.
- Cover happy path, boundary conditions, selection/tie-break branches, and fallback paths.
- Cover provider exception handling paths.
- Keep tests independent; no shared mutable state.
- Use clear test names: `Method_Scenario_ExpectedOutcome`.

## 8. Documentation And Comments

- Add XML summaries for public classes/interfaces/methods.
- Keep comments purposeful: explain intent or non-obvious decisions.
- Remove stale comments during refactors.
- Keep prompts.md and reflection.md updated at each phase checkpoint.
- Keep assumptions explicitly documented in BRD.md/README.md where required.

## 8.1 Deploy And Runbook Standards

- README.md must let a clean-clone user run backend, tests, and frontend without guessing.
- Commands must be copy-pasteable and version checks explicit.
- Document default ports, API base URL, and how to change environment settings.
- Include smoke-test examples that cover each unified status and unknown case.

## 9. Security And Compliance

- No real APIs for providers; stubs only.
- No credentials, keys, or secrets in code or committed files.
- Keep deterministic stub behavior for repeatable tests.

## 10. Pre-Commit Checklist

Before commit, verify:
- Code builds with zero errors.
- Contracts still match spec.md.
- Logging and validation behavior remain compliant.
- Added/changed logic has corresponding tests.
- No secrets introduced.
- prompts.md and reflection.md updated if the phase changed.
- `spec.md` commit ordering requirement is still satisfied.

## 11. Evaluator Alignment Checklist

Use this quick gate before submission:
- Analyze: assumptions stated and traceable in BRD.md.
- Architect: provider abstraction clean and dependency-injected.
- Design: data model defined before implementation and committed first.
- Develop: build passes and implementation matches spec contract.
- Test: normalization and provider-selection rules covered by unit tests.
- Deploy: clean-clone startup instructions verified end-to-end.
- Operate: provider failures handled gracefully with meaningful logs.
- Document: prompts.md complete, README present, assumptions visible.
