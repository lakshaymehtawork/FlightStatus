# BRD - Flight Status Lookup Feature

## 1. Executive Summary / Purpose
The purpose of this initiative is to deliver a low-risk, full-stack Flight Status lookup capability for SkyRoute within a one-day trial timeline. The feature allows a support agent to search by flight number and date, aggregate data from two providers, and present a normalized unified status to end users. This BRD captures business, functional, and non-functional requirements directly from the assignment brief and defines assumptions where the brief is silent. The document is designed to be fully traceable and testable for evaluator review.

## 2. Background / Problem Statement
SkyRoute requires a reliable way for support agents to look up flight status from heterogeneous providers with different schemas and status vocabularies. Without normalization and selection logic, support users may receive inconsistent or incomplete status outcomes. The solution must consolidate provider responses into a single status model and present it through backend and frontend components with clear error handling and documentation.

## 3. Scope
### 3.1 In-Scope
- Build an end-to-end Flight Status lookup flow for one scenario: input flight number + date, output unified status result.
- Implement backend endpoint `GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}` using .NET Minimal API.
- Integrate exactly two deterministic stub providers: AeroTrack and QuickFlight.
- Normalize provider-specific statuses into a unified enum: OnTime, Delayed, Cancelled, Diverted, Unknown.
- Treat date as a first-class lookup input by validating the format, preserving it through the request/response flow, and avoiding hidden min/max date windows or other undocumented date filters.
- Apply provider-result selection rule using later `lastUpdatedUtc` when both providers respond.
- Return unified `FlightStatusResult` from backend.
- Implement basic backend input validation for required flight number and date.
- Build frontend search form, result card with required color coding, conditional AeroTrack-only fields, and basic API error state.
- Provide complete submission artifacts: README.md, spec.md, BRD.md, backend project, test project, frontend project, prompts.md, reflection.md.
- Use AI assistance and capture significant prompts in prompts.md.

### 3.2 Out-of-Scope
- Real external flight data APIs or live provider credentials.
- Authentication and authorization flows.
- Persistent storage/database design.
- Historical analytics, reporting, or dashboards.
- Multi-flight batch search.
- SLA-backed production infrastructure and observability tooling beyond assignment needs.
- Any additional providers beyond AeroTrack and QuickFlight.

## 4. Stakeholders
- Support Agent: primary end user entering flight number/date and viewing status outcome.
- Assignment Evaluator (agentic evaluator): assesses quality across eight SDLC dimensions.
- Developer/Submitter: implements and documents the solution and AI usage.
- SkyRoute Product/Engineering Stakeholders (implied): consumers of a maintainable, demonstrable feature prototype.

## 5. Source Sentence Index (for Traceability)
- SRC-001: "A low-stakes, full-stack trial run."
- SRC-002: "You have one day to take a simple scenario from brief to running application - analysis, architecture, design, code, tests, deployment steps, and documentation."
- SRC-003: "There is no right framework or folder layout."
- SRC-004: "No starter code."
- SRC-005: "Your choices are the submission."
- SRC-006: "The agentic evaluator will score your work across eight SDLC dimensions and return specific feedback."
- SRC-007: "Use that feedback to fix gaps before the timed challenge."
- SRC-008: "The SkyRoute platform needs a Flight Status lookup feature."
- SRC-009: "A support agent enters a flight number and a date."
- SRC-010: "The system queries two flight data providers, normalises their responses into a single status model, and displays the result."
- SRC-011: "AeroTrack returns full status detail: flight status, scheduled and actual departure/arrival times, terminal, gate, delay reason (when delayed)."
- SRC-012: "Response is verbose and uses its own field naming."
- SRC-013: "QuickFlight returns minimal status: flight status and scheduled times only."
- SRC-014: "No gate or terminal."
- SRC-015: "No delay reason."
- SRC-016: "Faster but less detailed."
- SRC-017: "Both providers use different status vocabularies."
- SRC-018: "Your normalisation layer must map both to a single unified status enum."
- SRC-019: "Unified status: OnTime, Delayed, Cancelled, Diverted, Unknown."
- SRC-020: "OnTime meaning: Departing/arrived within 15 minutes of schedule."
- SRC-021: "Delayed meaning: Departure or arrival pushed beyond 15 minutes."
- SRC-022: "Cancelled meaning: Flight will not operate."
- SRC-023: "Diverted meaning: Flight landed at a different airport."
- SRC-024: "Unknown meaning: Provider returned no usable status."
- SRC-025: "When both providers return a result, prefer the one with the later lastUpdatedUtc timestamp."
- SRC-026: "When only one provider responds, use that result."
- SRC-027: "When neither responds, return Unknown with an appropriate message."
- SRC-028: "You are expected to leverage GitHub Copilot for code generation, refactoring, and documentation throughout this challenge."
- SRC-029: "Please annotate your code or provide a summary indicating where Copilot was used and how it influenced your solution."
- SRC-030: "Submit your solution as a GitHub repository or a zip file containing all source code, documentation, and any supporting files."
- SRC-031: "Include a README file summarizing your approach, architectural decisions, and Copilot usage."
- SRC-032: "Follow any provided naming conventions and ensure your code is well-documented and professional."
- SRC-033: "Each dimension will be scored as pass/partial/fail with written feedback."
- SRC-034: "Backend (.NET Minimal API)."
- SRC-035: "GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}."
- SRC-036: "Calls both providers (use stubs - no real APIs)."
- SRC-037: "Normalises responses."
- SRC-038: "Returns unified FlightStatusResult."
- SRC-039: "Provider abstraction: IFlightStatusProvider with two concrete stub implementations."
- SRC-040: "Providers are injected via DI - the endpoint does not reference concrete types."
- SRC-041: "Basic input validation: flight number and date are required; return 400 if missing."
- SRC-042: "Frontend (Choose your own)."
- SRC-043: "Search form: flight number input + date picker."
- SRC-044: "Result card: unified status with colour coding (green = OnTime, amber = Delayed, red = Cancelled/Diverted, grey = Unknown)."
- SRC-045: "AeroTrack-only fields (gate, terminal, delay reason) shown when present, hidden when absent."
- SRC-046: "Basic error state when the API returns an error."
- SRC-047: "Did you identify and state your assumptions before writing code?"
- SRC-048: "Is the provider abstraction clean and dependency-injected?"
- SRC-049: "Did you define your data model before implementing it?"
- SRC-050: "Does the code build, run, and implement the spec correctly?"
- SRC-051: "Are the normalisation rules and provider selection logic covered by unit tests?"
- SRC-052: "Can the application be started from a clean clone using your instructions?"
- SRC-053: "Does the system handle provider failures gracefully and log meaningfully?"
- SRC-054: "Document: Are prompts captured, README present, and assumptions noted?"
- SRC-055: "Submission structure includes README.md, spec.md, FlightStatus.Api, FlightStatus.Tests, flight-status-ui, prompts.md, reflection.md."
- SRC-056: "spec.md must be committed before any implementation files. The commit timestamp is checked by the evaluator."
- SRC-057: "Use AI assistance freely - this is expected and encouraged."
- SRC-058: "Capture every significant prompt in prompts.md."
- SRC-059: "Do not use real flight data APIs or live credentials."
- SRC-060: "Stub providers must be deterministic - hardcode a small set of test responses."
- SRC-061: "No real secrets in committed files."

## 6. Business Requirements
- BR-001: The solution shall deliver a full-stack Flight Status lookup capability for SkyRoute. Source: SRC-008.
- BR-002: The delivery shall include analysis, architecture, design, implementation, testing, deployment steps, and documentation artifacts within the assignment scope. Source: SRC-002.
- BR-003: The solution shall support support-agent workflow for entering flight number and date and obtaining a status result. Source: SRC-009, SRC-010.
- BR-004: The solution shall normalize heterogeneous provider responses into one business-facing unified status model. Source: SRC-010, SRC-017, SRC-018.
- BR-005: The solution shall be evaluated against eight SDLC dimensions and must contain artifacts needed for that evaluation. Source: SRC-006, SRC-033.
- BR-006: The submission package shall include source code, required documentation files, and supporting files in required structure. Source: SRC-030, SRC-055.
- BR-007: The solution shall document significant AI/Copilot influence and usage. Source: SRC-028, SRC-029, SRC-054, SRC-058.
- BR-008: The implementation shall avoid real provider integrations and secrets in repository artifacts. Source: SRC-059, SRC-061.

## 7. Functional Requirements
- FR-001: The system shall accept two user inputs for lookup: `flightNumber` and `date`. Source: SRC-009, SRC-043.
- FR-002: The backend shall expose HTTP GET endpoint `GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}`. Source: SRC-035.
- FR-003: The backend shall invoke both AeroTrack and QuickFlight providers for each valid request. Source: SRC-010, SRC-036.
- FR-004: The backend shall implement provider abstraction via `IFlightStatusProvider` with exactly two concrete stub implementations for AeroTrack and QuickFlight. Source: SRC-039.
- FR-005: Provider implementations shall be injected through dependency injection and endpoint code shall not reference concrete provider classes directly. Source: SRC-040.
- FR-006: The backend shall normalize provider-specific response vocabularies to unified statuses: OnTime, Delayed, Cancelled, Diverted, Unknown. Source: SRC-018, SRC-019.
- FR-007: Unified status OnTime shall represent departure/arrival within 15 minutes of schedule. Source: SRC-020.
- FR-008: Unified status Delayed shall represent departure or arrival pushed beyond 15 minutes of schedule. Source: SRC-021.
- FR-009: Unified status Cancelled shall represent that the flight will not operate. Source: SRC-022.
- FR-010: Unified status Diverted shall represent that the flight landed at a different airport. Source: SRC-023.
- FR-011: Unified status Unknown shall represent provider returned no usable status. Source: SRC-024.
- FR-012: If both providers return results, the backend shall select the provider result having the later `lastUpdatedUtc`. Source: SRC-025.
- FR-013: If only one provider returns a result, the backend shall use that result. Source: SRC-026.
- FR-014: If neither provider returns a result, the backend shall return unified status Unknown and include an appropriate message. Source: SRC-027.
- FR-015: Backend input validation shall require both `flightNumber` and `date`; if missing, API shall return HTTP 400. Source: SRC-041.
- FR-016: Backend shall return a unified `FlightStatusResult` payload for successful lookups. Source: SRC-038.
- FR-017: AeroTrack stub response model shall include flight status, scheduled departure/arrival, actual departure/arrival, terminal, gate, and delay reason (when delayed). Source: SRC-011.
- FR-018: AeroTrack integration logic shall support verbose provider schema and provider-specific field names before normalization. Source: SRC-012.
- FR-019: QuickFlight stub response model shall include only flight status and scheduled times. Source: SRC-013.
- FR-020: QuickFlight data shall not include gate, terminal, or delay reason fields. Source: SRC-014, SRC-015.
- FR-021: Frontend shall provide a search form with flight number input and date picker input. Source: SRC-043.
- FR-022: Frontend shall display a result card showing unified status with required color mapping: green OnTime, amber Delayed, red Cancelled/Diverted, grey Unknown. Source: SRC-044.
- FR-023: Frontend shall show AeroTrack-only fields (gate, terminal, delay reason) only when present and hide them when absent. Source: SRC-045.
- FR-024: Frontend shall display a basic error state when API returns an error response. Source: SRC-046.
- FR-025: Stub providers shall be deterministic using hardcoded small test-response sets. Source: SRC-060.
- FR-026: Solution shall include prompts.md with all significant prompts and notes. Source: SRC-058, SRC-055.
- FR-027: Solution shall include README.md summarizing approach, architecture decisions, and Copilot usage. Source: SRC-031.
- FR-028: Solution shall include reflection.md documenting evaluator feedback and subsequent fixes/differences. Source: SRC-055.

## 8. Non-Functional Requirements
- NFR-001 (Time Constraint): The assignment implementation shall be scoped to a one-day trial timeline. Source: SRC-002.
- NFR-002 (Build/Run Quality): Code shall build, run, and implement documented specification correctly. Source: SRC-050.
- NFR-003 (Architecture Quality): Provider abstraction shall be clean and dependency-injected. Source: SRC-048.
- NFR-004 (Testability): Unit tests shall cover normalization rules and provider selection logic. Source: SRC-051.
- NFR-005 (Operational Resilience): System shall handle provider failures gracefully and produce meaningful logs. Source: SRC-053.
- NFR-006 (Documentation Quality): Documentation shall be professional, clear, and complete for evaluator criteria. Source: SRC-031, SRC-032, SRC-054.
- NFR-007 (Security/Compliance): No real credentials, real external APIs, or real secrets shall appear in committed artifacts. Source: SRC-059, SRC-061.
- NFR-008 (Submission Professionalism): Submission structure and naming conventions shall align with provided instructions. Source: SRC-032, SRC-055.
- NFR-009 (Version Control Gate): `spec.md` shall be committed before any implementation files; commit timestamp order must prove this. Source: SRC-056.

## 9. Assumptions & Constraints (Gap Analysis)
- ASSUMPTION-001: Date input uses calendar date only in `yyyy-MM-dd`, interpreted in system local timezone for provider-query simulation.
- ASSUMPTION-002: Flight number format validation is minimal (non-empty string); strict IATA/ICAO regex validation is out of scope unless later specified.
- ASSUMPTION-003: Missing means null, empty, or whitespace for `flightNumber`; missing means null/empty query value for `date`.
- ASSUMPTION-004: Invalid `date` format (non-`yyyy-MM-dd`) returns HTTP 400 with a descriptive error payload.
- ASSUMPTION-005: No date range restriction is applied; any valid `yyyy-MM-dd` value is accepted as long as it passes format validation.
- ASSUMPTION-006: Stub providers use flight number as the deterministic lookup key and ignore date for matching, while still echoing the validated date through the result model.
- ASSUMPTION-007: Provider calls are executed concurrently to minimize latency and satisfy graceful-degradation expectations.
- ASSUMPTION-008: Each provider call has independent failure handling; exceptions/timeouts from one provider must not fail the overall request when another provider succeeds.
- ASSUMPTION-009: Per-provider timeout default is 2 seconds for stub simulation and resilience testability.
- ASSUMPTION-010: If both providers return identical `lastUpdatedUtc`, precedence defaults to AeroTrack for deterministic tie-breaking.
- ASSUMPTION-011: "Appropriate message" for Unknown when neither responds means human-readable explanation suitable for support agent display and logging.
- ASSUMPTION-012: HTTP status for neither-provider-data scenario is 200 with status Unknown (business no-data), unless explicitly changed later.
- ASSUMPTION-013: HTTP status for successful normalization from at least one provider is 200.
- ASSUMPTION-014: CORS policy will allow local frontend dev origin only (default `http://localhost:4200`), expandable by configuration.
- ASSUMPTION-015: Authentication/authorization is not required for this assignment feature.
- ASSUMPTION-016: Logging levels minimum: INFO for request/result lifecycle and WARN for provider failures.
- ASSUMPTION-017: Serialization format for datetime values is ISO 8601 UTC where timestamp fields are returned.
- ASSUMPTION-018: No persistence is required; all provider data comes from deterministic in-memory stubs.

Constraints derived from brief:
- CONSTRAINT-001: Must use stubs only, not real flight APIs. Source: SRC-036, SRC-059.
- CONSTRAINT-002: Must maintain deterministic stub behavior. Source: SRC-060.
- CONSTRAINT-003: Must include required artifact structure and files. Source: SRC-055.
- CONSTRAINT-004: Must keep `spec.md` commit timestamp before implementation files. Source: SRC-056.

## 10. Risks / Open Questions
- RISK-001: Ambiguous tie-break behavior when both providers have same `lastUpdatedUtc` may cause inconsistent implementations without explicit rule.
- RISK-002: "Appropriate message" content for Unknown is not formally specified and may vary across implementations.
- RISK-003: Provider timeout thresholds are unspecified, potentially affecting resilience test outcomes.
- RISK-004: Lack of explicit invalid-format error schema may produce evaluator mismatch if response shape expectations differ.
- RISK-005: Color rendering may vary across UI themes unless exact color tokens/codes are standardized in design phase.

Resolved interpretation from assignment brief (no further blocking questions):
- DEC-001: When neither provider responds, return unified status Unknown with an appropriate message as a business outcome in the normal response flow.
- DEC-002: Selection rule is explicitly based on later `lastUpdatedUtc` when both providers respond.
- DEC-003: If both providers return identical `lastUpdatedUtc`, apply ASSUMPTION-008 (deterministic AeroTrack precedence) unless superseded in a later phase decision.

## 11. Acceptance / Success Criteria
- AC-001: User can submit flight number and date and receive a unified flight status result via API and UI.
- AC-002: Backend endpoint path and query contract match exact required format.
- AC-003: Both providers are queried via DI abstraction, without endpoint-level concrete references.
- AC-004: Normalization supports all required unified statuses and definitions, including 15-minute threshold handling.
- AC-005: Provider selection follows later `lastUpdatedUtc` rule when both respond.
- AC-006: If one provider fails/returns no data, system still returns result from the other when available.
- AC-007: If neither provider provides data, system returns Unknown with user-meaningful message.
- AC-008: Required input validation returns HTTP 400 when `flightNumber` or `date` is missing.
- AC-009: Frontend status color coding and conditional AeroTrack field visibility match stated requirements.
- AC-010: Frontend displays basic API error state for failed requests.
- AC-011: Deterministic stubs, no real APIs, and no real secrets are verified in repository.
- AC-012: Documentation set is complete (README, spec, BRD, prompts, reflection) and includes AI usage notes and assumptions.
- AC-013: `spec.md` commit predates implementation file commits in git history.


