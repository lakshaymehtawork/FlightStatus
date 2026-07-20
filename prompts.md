# prompts.md — AI Prompt Log

All significant GitHub Copilot prompts used during this project, with accept/reject notes.
Required by evaluator per question.txt: "Capture every significant prompt in prompts.md."

---

### Phase 0 — Starter / Context Lock

**Prompt summary:** Sent the Phase 0 STARTER prompt from prompts-master.md to establish the project context, lock the stack (C# .NET 10 Minimal API, xUnit + Moq, Angular), and confirm the required output structure. Asked Copilot to summarize question.txt and flag critically unclear items before any work began.

**Accepted:** Summary of the scenario (flight number + date → two providers → normalized status → Angular UI). Copilot correctly identified the tie-break ambiguity (equal lastUpdatedUtc) as the only genuinely unclear item.

**Rejected / modified:** Nothing rejected. The tie-break question was resolved using Question.txt as authority: AeroTrack wins on equal timestamps (explicit assumption, not stated in brief).

**AI influence:** Shaped the initial understanding of scope and flagged the one ambiguity that became ASSUMPTION-008 in BRD.md.

---

### Phase 1 — Analyze (BRD.md)

**Prompt summary:** Sent the Phase 1 ANALYZE prompt. Copilot was asked to act as a Senior BA, read Question.txt sentence by sentence, assign FR-#/BR-# IDs to every requirement, perform gap analysis, and produce BRD.md.

**Accepted:** Full BRD structure including source sentence index (SRC-001..061), all FR/BR/NFR entries, 16 explicit assumptions covering concurrency, CORS, auth, logging, serialization, tie-break behavior, and a resolved-decisions section removing blocking questions.

**Rejected / modified:** Initial output included two blocking questions (tie-break and HTTP 200 vs non-200 for Unknown). User confirmed Question.txt answers both — updated to DEC-001..003 resolved decisions, removing the blocking question section.

**AI influence:** AI generated the entire BRD.md content including SRC traceability index, gap analysis, and assumption set. Reviewed and confirmed correct by user.

---

### Phase 2 — Design (spec.md)

**Prompt summary:** Sent the Phase 2 DESIGN prompt. Copilot was asked to produce the complete technical contract: data model with nullability, provider raw schemas, interface signatures, API contract with JSON examples, and 38 testable business rules (BR-RULE-001..038).

**Accepted:** Full spec.md including FlightStatusQuery, FlightStatusResult, ProviderFlightStatusCandidate, ApiError entities; IFlightStatusProvider and IFlightStatusService interfaces; exact HTTP contract with request/response JSON examples for success and Unknown cases; all error shapes; BR-RULE-001..038 covering validation, AeroTrack and QuickFlight mapping tables, selection/fallback rules, fault tolerance rules, field population rules, frontend display contract, and security rules.

**Rejected / modified:** Confirmation required section at the end was replaced with resolved decisions after user confirmed all assumptions from Question.txt. Raw provider vocabulary names (AeroTrackStatusRaw, QuickFlightStatusRaw) were kept as defined in Section 3.4 of spec.

**AI influence:** AI generated entire spec.md content. spec.md was committed to git before any implementation files to satisfy evaluator timestamp requirement.

---

### Phase 3 — Architect (architecture.md)

**Prompt summary:** Sent the Phase 3 ARCHITECT prompt. Copilot was asked to choose the simplest architecture style, define layers and dependency direction, select justified design patterns with SOLID mapping, list avoided patterns, produce exact DI registration lines, complete folder/file layout, and a Mermaid sequence diagram.

**Accepted:** Layered architecture (API → Application → Domain ← Infrastructure). Three patterns: Strategy (IFlightStatusProvider), Adapter (per-provider raw→candidate mapping), DI (endpoint→service→providers). Four avoided patterns: Repository/UoW, CQRS/Mediator, Factory, Event Sourcing. Exact DI registration lines for Program.cs. Full folder structure for all three projects. Mermaid diagram updated to show Task.WhenAll concurrent calls and per-provider exception logging.

**Rejected / modified:** Initial Mermaid diagram did not show concurrent calls or the WARN exception path. Updated in a follow-up to add `note over SVC: Task.WhenAll` and exception WARN annotation. Angular component file listing was refined to show exact file names (`.component.ts`, `.component.html`, `.component.scss`) and environment files.

**AI influence:** AI generated architecture.md in full. Folder structure used directly as implementation blueprint in Phase 4.

---

### Phase 4 — Develop Backend (FlightStatus.Api)

**Prompt summary:** Sent the Phase 4 DEVELOP (Backend) prompt. Copilot was asked to implement all source files under FlightStatus.Api per architecture.md: domain layer (enums, models, abstractions), application service with concurrent provider calls and structured logging, infrastructure stub providers with deterministic datasets, and Program.cs with DI, CORS, OpenAPI, input validation, and global error handler.

**Accepted:** All 14 source files created and building successfully:
- Domain layer: UnifiedFlightStatus, FlightStatusQuery, FlightStatusResult, ProviderFlightStatusCandidate, IFlightStatusProvider, IFlightStatusService.
- Contracts: ApiError record.
- Application: FlightStatusService with Task.WhenAll, SafeGetStatusAsync per-provider exception isolation, INFO/WARN structured logging, and alphabetical tie-break selecting AeroTrack.
- Infrastructure: AeroTrackStubProvider (5 stubs, BR-RULE-004..009 mapping), QuickFlightStubProvider (6 stubs, BR-RULE-010..015 mapping), covering OnTime/Delayed/Cancelled/Diverted/Unknown and all selection/fallback/tie-break scenarios.
- Program.cs: JSON string enum serialization, CORS from appsettings, Scalar UI for OpenAPI, global exception handler returning ApiError 500, endpoint with boundary input validation returning ApiError 400.

**Rejected / modified:** Scalar.AspNetCore added (implied by "Enable API documentation in development"). Attempt to suppress NU1903 advisory on Microsoft.OpenApi 2.0.0 via direct package reference rejected — it caused a version downgrade conflict. Advisory retained as known-accepted warning (no patched stable version available in .NET 10 ecosystem at time of build; no runtime impact for this assignment).

**AI influence:** AI generated all 14 backend source files and appsettings updates. Build confirmed zero errors. Stub dataset of 8 flights (SR100..SR800) designed to cover all BR-RULE branches needed by Phase 5 tests.

---

### Phase 5 — Test (FlightStatus.Tests)

**Prompt summary:** Sent the Phase 5 TEST prompt. Copilot was asked to create focused, independent tests under `FlightStatus.Tests` to validate spec rules, including mapping tables, selection/tie-break branches, fallback behavior, and endpoint validation/error paths.

**Accepted:** Added architecture-aligned test structure and files:
- `Unit/Application/FlightStatusServiceTests.cs`
- `Unit/Infrastructure/AeroTrackStubProviderTests.cs`
- `Unit/Infrastructure/QuickFlightStubProviderTests.cs`
- `Integration/FlightStatusEndpointTests.cs`
- `TestDoubles/ThrowingProviderStub.cs`

Implemented and executed tests for:
- Provider selection branches (newer timestamp, one provider only, neither provider).
- Tie-break branch (equal timestamps => AeroTrack).
- Provider exception fallback behavior.
- Mapping rows present in deterministic stubs (ON_SCHEDULE/DELAYED/CANCELED/DIVERTED, ON_TIME/LATE/CANCELLED/DIVERTED).
- Endpoint validation for missing `flightNumber`, missing `date`, and invalid date format.
- Unknown business outcome response path.

**Rejected / modified:** `runTests` tool did not discover tests from explicit file paths in this workspace, so execution was switched to `dotnet test` directly. Added `public partial class Program;` to `Program.cs` to support in-process integration tests via `WebApplicationFactory<Program>`.

**AI influence:** AI authored all test files and test double, removed the placeholder `UnitTest1.cs`, ran the suite, and iterated until green.

---

### Phase 6 — Frontend (flight-status-ui)

**Prompt summary:** Sent the Phase 6 FRONTEND prompt. Copilot was asked to implement the Angular 22 frontend in the pre-scaffolded `flight-status-ui/` project without re-scaffolding. Requirements: environment files for API base URL, models/enum mirroring spec.md types, HTTP service using HttpClient, search-form component with reactive forms and inline validation, status-result component with color-coded status badge and conditional AeroTrack fields, app shell owning state (result/isLoading/error), `provideHttpClient()` in app config, and a zero-error production build.

**Accepted:** All frontend files created or updated:
- `src/environments/environment.ts` and `environment.development.ts` — API base URL; dev swap via `fileReplacements` in angular.json.
- `src/app/models/unified-flight-status.ts` — TypeScript enum mirroring `UnifiedFlightStatus` backend enum.
- `src/app/models/flight-status-result.ts` — Interface with all fields nullable as `T | null` matching spec.md.
- `src/app/services/flight-status-api.service.ts` — `inject(HttpClient)`, URL from environment, single `getFlightStatus()` method, errors propagate to subscriber.
- `src/app/components/search-form/` — `ReactiveFormsModule` with required/minLength validators, date picker, submit disabled on `isLoading` signal input, `output<SearchQuery>()` event, inline field error spans.
- `src/app/components/status-result/` — signal `input<FlightStatusResult | null>(null)`, `[ngClass]` color mapping (green/amber/red/grey), `DatePipe` for timestamps, conditional rendering of all AeroTrack-only fields.
- Updated `app.ts` — shell owns `signal<>` state, subscribes to service, catches `HttpErrorResponse`.
- Updated `app.html` — error banner, loading overlay with spinner, `<app-search-form>` and `<app-status-result>` bindings.
- Updated `app.config.ts` — added `provideHttpClient()`.
- Updated `styles.scss` — CSS custom properties, reset, body font.
- Updated `angular.json` — `fileReplacements` for development environment swap.

**Rejected / modified:** No third-party UI libraries used (no Angular Material, PrimeNG, etc.). No NgRx state management. Production build completed successfully: main bundle ~295 kB, zero errors, zero type errors, angular.json budget warnings within limits.

**AI influence:** AI generated all 16 frontend files, resolved the `NgClass` missing-import issue (initially omitted from `status-result.component.ts` imports array), and ran the production build to confirm zero errors.

---

### Phase 6.5 — Frontend Test Hardening (flight-status-ui)

**Prompt summary:** Sent a dedicated follow-up prompt to create comprehensive frontend tests without changing runtime behavior. Copilot was asked to cover service-layer HTTP behavior, component-level validation/rendering, app-shell/container state handling, and non-2xx error paths.

**Accepted:** Added and updated Angular/Vitest specs:
- `src/app/services/flight-status-api.service.spec.ts`
- `src/app/components/search-form/search-form.component.spec.ts`
- `src/app/components/status-result/status-result.component.spec.ts`
- Updated shell/feature-level specs to verify loading, success, and error states.

Executed frontend test suite and confirmed all tests pass (`14/14`).

**Rejected / modified:** Browser-computed CSS color values were not asserted in unit tests because jsdom-style environments are better suited for class/assertion validation than pixel-accurate visual rendering checks.

**AI influence:** AI generated the complete frontend testing layer and iterated one failing spec-import issue to green.

---

### Phase 6.x — Frontend Architecture and UX Refinement

**Prompt summary:** Sent a prompt to improve maintainability and UI quality while preserving behavior. Copilot was asked to avoid business-logic changes and refactor toward cleaner component boundaries.

**Accepted:** Refactored Angular structure to a thin host root + dedicated feature container:
- New feature container: `src/app/features/flight-status-page/flight-status-page.component.*`
- `app-root` reduced to a host wrapper rendering the feature component.
- Behavior tests moved to the feature container spec.

Additional visual-only redesign applied to HTML/SCSS for an aviation operations dashboard theme (dark navy, cyan accents, glass panels, radar/grid motifs, responsive spacing).

**Rejected / modified:** No changes made to API calls, routing semantics, form validators, business rules, or data-binding contracts.

**AI influence:** AI performed a non-breaking componentization pass and a purely visual premium redesign while keeping test/build green.

---

### Phase 7 — Deploy (README.md)

**Prompt summary:** Sent the Phase 7 DEPLOY prompt to replace placeholder README with clean-clone operational instructions.

**Accepted:** README now includes:
- Prerequisites with minimum versions, verification commands, and install links.
- Exact copy-paste commands for backend run, tests, and frontend run.
- Configuration section (ports, CORS, environment files, API base URL updates).
- Manual smoke-test matrix covering OnTime/Delayed/Cancelled/Diverted/Unknown and partial-provider scenarios.
- Full assumptions list copied from BRD for evaluator quick-reference.

**Rejected / modified:** None; followed prompt constraints and kept commands explicit with no ambiguous placeholders.

**AI influence:** AI authored complete README deployment/runbook content and aligned it to actual repository state.

---

### Phase 8 (Partial) — Operate/Documentation Follow-ups

**Prompt summary:** Follow-up operational/debug prompts were used to resolve runtime behavior in local UI testing and update docs continuity.

**Accepted:**
- Root-caused and fixed local UI CORS failure pattern (`307` redirect followed by browser CORS error) by applying HTTPS redirection only outside Development in `Program.cs`.
- Re-validated end-to-end UI → API request/response path.
- Continued updates to `prompts.md` and `reflection.md` as phases progressed.

**Rejected / modified:** Did not introduce new dependencies or non-required platform features during operational fixes.

**AI influence:** AI handled troubleshooting, safe middleware adjustment, and documentation synchronization.

---

### Phase 8.x — Date Validation Hotfix

**Prompt summary:** Asked Copilot to stop obviously nonsensical ancient dates from being accepted by the UI and API after a user reported `1000-03-20` being submitted and returning a misleading result.

**Accepted:** Added a shared minimum supported date guard (`1900-01-01`) in the Angular search form and backend endpoint, plus regression tests covering both validation layers.

**Rejected / modified:** Did not change the provider selection rules or stub datasets; the fix is validation-only and keeps the normal lookup flow intact for supported dates.

**AI influence:** Copilot produced the validation pattern, test cases, and messaging; I kept the change narrow to avoid altering unrelated business logic.

---

### Phase 8.y — Date Format Switch

**Prompt summary:** Adjusted the date contract again so the user-facing input is `DD-MM-YYYY` instead of ISO-style text, while keeping the backend response model unchanged.

**Accepted:** Updated the Angular field to a text input with `DD-MM-YYYY` placeholder and rewrote backend parsing/validation to accept `dd-MM-yyyy`.

**Rejected / modified:** Did not change the API response serialization or the underlying `DateOnly` domain model; only the inbound request format changed.

**AI influence:** Copilot supplied the validator rewrite and test updates; I verified the input parsing behavior against the endpoint tests.

