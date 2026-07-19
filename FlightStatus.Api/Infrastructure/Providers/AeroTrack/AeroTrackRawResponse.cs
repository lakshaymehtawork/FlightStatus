namespace FlightStatus.Api.Infrastructure.Providers.AeroTrack;

/// <summary>
/// Represents the verbose, provider-specific raw response schema from AeroTrack.
/// Field names mirror AeroTrack's own naming convention (e.g. sched_dep_utc, gate_code).
/// Mapped to ProviderFlightStatusCandidate before leaving the infrastructure layer.
/// </summary>
public record AeroTrackRawResponse(
    string FlightStatus,           // flight_status
    DateTime? SchedDepUtc,         // sched_dep_utc
    DateTime? SchedArrUtc,         // sched_arr_utc
    DateTime? ActualDepUtc,        // actual_dep_utc
    DateTime? ActualArrUtc,        // actual_arr_utc
    string? TerminalCode,          // terminal_code
    string? GateCode,              // gate_code
    string? DelayReasonText,       // delay_reason_text (present only when delayed)
    DateTime LastUpdatedUtc        // last_updated_utc
);
