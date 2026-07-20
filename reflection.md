# reflection.md

_Updated progressively as each SDLC phase completes. Final version will be written after Phase 8._

---

## Self-Assessment Against Evaluator Dimensions

| Dimension | Status | Reason |
|---|---|---|
| **Analyze** | Pass | BRD.md traces every sentence in Question.txt to a FR-#/BR-# with SRC-001..061 index; 16 explicit assumptions with labels; IN/OUT-OF-SCOPE lists present; gap analysis complete. |
| **Design** | Pass | spec.md committed to git (commit a7f1df3) before any implementation files; all types, fields, nullability, and 38 testable BR-RULE IDs defined; API contract with JSON examples included. |
| **Architect** | Pass | Provider abstraction via IFlightStatusProvider; Strategy+Adapter patterns with SOLID justifications; avoided patterns listed with reasons; exact DI registrations in architecture.md; Mermaid sequence diagram shows concurrent calls and fault path. |
| **Develop** | Pass | Project builds zero errors; endpoint path matches spec (`GET /flights/status`); endpoint depends only on IFlightStatusService (never concrete types); both stubs deterministic; concurrent Task.WhenAll; per-provider exception isolation; structured INFO/WARN logging; input validation returns 400 with ApiError. |
| **Test** | Pass (with noted gaps) | 26 tests implemented and passing across unit + integration layers; core selection/fallback/validation/mapping branches covered. A small subset of spec rules remains not directly coverable due current implementation and stub dataset limits. |
| **Deploy** | Not started | README.md placeholder only; Phase 7 not yet executed. |
| **Operate** | Partial | Provider fault tolerance and logging implemented in Phase 4. Full Phase 8 resilience review not yet done. |
| **Document** | Partial | prompts.md and reflection.md populated with Phases 0–6. BRD.md and spec.md complete. README.md still placeholder. |

---

## What Was Changed After Review

### Phase 1 review
- Removed blocking question section and replaced with DEC-001..003 resolved decisions after user confirmed Question.txt answers both open questions.

### Phase 2 review
- Replaced "Confirmation Required From User" section with "Resolved Contract Decisions" — all three assumptions finalized from brief authority.

### Phase 3 review
- Mermaid sequence diagram updated to show concurrent provider calls via `Task.WhenAll` and the exception WARN path.
- Angular component file listing refined from wildcard `/*` to exact filenames per component.

### Phase 4 review
- Attempted to fix NU1903 (Microsoft.OpenApi 2.0.0 vulnerability) via direct package pin — rejected because no higher stable patch exists in .NET 10 ecosystem; accepted as known warning.
- appsettings.json updated to include CORS origins section so frontend origin is configurable without code changes.

### Phase 5 review
- Added 5 test files under architecture-defined folders and removed placeholder `UnitTest1.cs`.
- Added `public partial class Program;` in backend entry point to enable integration tests with `WebApplicationFactory<Program>`.
- Executed full suite via `dotnet test`: total 26, passed 26, failed 0.

### Phase 6 review
- Implemented full Angular 22 frontend: environment files, models, HTTP service, search-form component, status-result component, app shell, global styles.
- Color coding: green (OnTime), amber (Delayed), red (Cancelled/Diverted), grey (Unknown) via CSS class mapping.
- All AeroTrack-only fields (terminal, gate, actualDepartureUtc, actualArrivalUtc, delayReason) conditionally shown only when non-null (BR-RULE-034).
- Fixed missing `NgClass` import in `StatusResultComponent` caught during build review.
- Production build: zero errors, main bundle ~295 kB, within angular.json budget thresholds.

---

## What I Would Do Differently With More Time

1. **Add integration test for forced provider exception path** by overriding DI in a custom test host so endpoint-level resilience can be verified under real HTTP execution.
2. **Pin Microsoft.OpenApi to a patched version** once one is released for .NET 10 to eliminate the NU1903 security advisory cleanly.
3. **Add request correlation ID middleware** so the `requestId` field in ApiError is automatically set from an `X-Request-ID` header for better distributed tracing.
4. **Configure structured JSON logging** (e.g. via Serilog or `JsonConsoleFormatter`) so log output is machine-parseable in CI/CD pipelines.
5. **Add a `date` path through the stub lookup** so stubs could be keyed by both flight number and date — currently date is ignored, which is sufficient for the assignment but would be unrealistic for a real stub.

---

## AI / Copilot Usage Summary

- **Phases 0–4 completed with GitHub Copilot (Claude Sonnet 4.6) via VS Code Chat.**
- Time saved: BRD traceability matrix with 61 source sentences, 38 testable business rules, data model tables, and 14 backend source files were all AI-generated and required only targeted review and confirmation rather than authoring from scratch.
- Corrections needed: Mermaid diagram required one refinement pass (concurrent call annotations). Package version tie-break required manual resolution. Blocking-question sections required user confirmation and replacement with resolved-decision sections.
- Overall assessment: AI assistance was highly effective for SDLC artifact generation and boilerplate-free implementation. Human review remained essential for cross-checking assumptions, resolving ambiguities, and confirming spec compliance.

---

## Phase 6.5 / 7 / 8 Addendum (Appended - Existing Content Preserved)

### Additional work completed after Phase 6
- Added frontend-focused tests in Angular for:
	- API service behavior (request shape + error propagation)
	- Search form validation and submit behavior
	- Status result rendering and conditional field visibility
	- Container-level loading/success/error flow
- Frontend test suite passes (`14/14`).

### Frontend maintainability and UX refinement
- Refactored Angular structure to a thin host `app-root` and a dedicated feature container component (`flight-status-page`) without changing business behavior.
- Applied a premium aviation-themed UI redesign using HTML/SCSS only:
	- dark navy dashboard background
	- cyan/blue accent tokens
	- glassmorphism cards
	- radar/grid visual motif
	- responsive layout improvements
- Preserved API calls, form controls/validation, bindings, and data flow.

### Deploy documentation completion
- Replaced placeholder `README.md` with clean-clone run instructions including:
	- prerequisites + version checks + install links
	- exact copy-pasteable backend, test, and frontend commands
	- default ports/config details
	- smoke-test matrix for all key status scenarios
	- assumptions quick reference

### Operate/resilience follow-up
- Diagnosed and resolved local runtime CORS issue caused by HTTP->HTTPS redirect (`307` then browser CORS block).
- Updated backend middleware behavior so HTTPS redirection is not applied in Development.
- Strengthened log context by including request identifier in request-entry and provider result/exception logs.
- Revalidated backend tests after operational logging updates: `26/26` passing.

### Date validation hotfix
- Added a minimum supported date guard (`1900-01-01`) in both the Angular search form and the backend endpoint after a user reported `1000-03-20` being accepted and returning a misleading result.
- Added regression coverage for the rejected date path in both the frontend and integration tests.

### Date format switch
- Changed the inbound date format from ISO-style input to `DD-MM-YYYY` to match the user-facing requirement, while leaving the backend response model as a `DateOnly`-backed ISO JSON value.
- Updated the search form to a text input with `DD-MM-YYYY` placeholder and validated both format and minimum supported date in code.

### Updated dimension snapshot (latest)
- Analyze: Pass
- Design: Pass
- Architect: Pass
- Develop: Pass
- Test: Pass (backend + frontend suites green)
- Deploy: Pass (README completed)
- Operate: Partial (core resilience implemented; deeper production hardening still possible)
- Document: Partial (reflection updated; prompts log should be finalized strictly from user-provided prompt inventory)

