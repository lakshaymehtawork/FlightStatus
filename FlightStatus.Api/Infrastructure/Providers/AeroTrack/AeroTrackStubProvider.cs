using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;

namespace FlightStatus.Api.Infrastructure.Providers.AeroTrack;

/// <summary>
/// Deterministic stub implementation of IFlightStatusProvider for AeroTrack.
/// Maps AeroTrack's verbose snake_case schema to ProviderFlightStatusCandidate.
///
/// Stub dataset covers:
///   SR100 - OnTime;  both providers respond, AeroTrack has later timestamp (AeroTrack wins)
///   SR200 - Delayed; AeroTrack only (QuickFlight absent), includes delayReason
///   SR400 - Diverted; both respond, same timestamp (AeroTrack wins tie-break)
///   SR600 - OnTime;  both respond, QuickFlight has later timestamp (QuickFlight wins)
///   SR700 - Cancelled; both respond, same timestamp (AeroTrack wins tie-break)
///   SR300, SR500, SR800 - absent (returns null) to support partial/total failure tests
/// </summary>
public sealed class AeroTrackStubProvider : IFlightStatusProvider
{
    public string ProviderName => "AeroTrack";

    // Keyed by flight number (case-insensitive); date is ignored for stub simplicity
    private static readonly IReadOnlyDictionary<string, AeroTrackRawResponse> Stubs =
        new Dictionary<string, AeroTrackRawResponse>(StringComparer.OrdinalIgnoreCase)
        {
            // SR100: OnTime — AeroTrack has later timestamp than QuickFlight, so AeroTrack wins
            ["SR100"] = new(
                FlightStatus:    "ON_SCHEDULE",
                SchedDepUtc:     new DateTime(2026, 7, 20,  7, 30, 0, DateTimeKind.Utc),
                SchedArrUtc:     new DateTime(2026, 7, 20,  9, 45, 0, DateTimeKind.Utc),
                ActualDepUtc:    new DateTime(2026, 7, 20,  7, 25, 0, DateTimeKind.Utc),
                ActualArrUtc:    new DateTime(2026, 7, 20,  9, 40, 0, DateTimeKind.Utc),
                TerminalCode:    "T1",
                GateCode:        "A1",
                DelayReasonText: null,
                LastUpdatedUtc:  new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc)),

            // SR200: Delayed — AeroTrack only (QuickFlight absent), full detail with delayReason
            ["SR200"] = new(
                FlightStatus:    "DELAYED",
                SchedDepUtc:     new DateTime(2026, 7, 20,  7, 30, 0, DateTimeKind.Utc),
                SchedArrUtc:     new DateTime(2026, 7, 20,  9, 45, 0, DateTimeKind.Utc),
                ActualDepUtc:    new DateTime(2026, 7, 20,  9, 15, 0, DateTimeKind.Utc),
                ActualArrUtc:    null,
                TerminalCode:    "T2",
                GateCode:        "B3",
                DelayReasonText: "Crew rotation delay",
                LastUpdatedUtc:  new DateTime(2026, 7, 20,  9,  0, 0, DateTimeKind.Utc)),

            // SR400: Diverted — same timestamp as QuickFlight; AeroTrack wins tie-break
            ["SR400"] = new(
                FlightStatus:    "DIVERTED",
                SchedDepUtc:     new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc),
                SchedArrUtc:     new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc),
                ActualDepUtc:    new DateTime(2026, 7, 20,  8,  5, 0, DateTimeKind.Utc),
                ActualArrUtc:    null,
                TerminalCode:    "T3",
                GateCode:        "C5",
                DelayReasonText: null,
                LastUpdatedUtc:  new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc)),

            // SR600: OnTime — QuickFlight has later timestamp; QuickFlight wins
            ["SR600"] = new(
                FlightStatus:    "ON_SCHEDULE",
                SchedDepUtc:     new DateTime(2026, 7, 20,  6,  0, 0, DateTimeKind.Utc),
                SchedArrUtc:     new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc),
                ActualDepUtc:    null,
                ActualArrUtc:    null,
                TerminalCode:    "T1",
                GateCode:        "A2",
                DelayReasonText: null,
                LastUpdatedUtc:  new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc)),

            // SR700: Cancelled — same timestamp as QuickFlight; AeroTrack wins tie-break
            ["SR700"] = new(
                FlightStatus:    "CANCELED",
                SchedDepUtc:     new DateTime(2026, 7, 20, 14,  0, 0, DateTimeKind.Utc),
                SchedArrUtc:     new DateTime(2026, 7, 20, 16,  0, 0, DateTimeKind.Utc),
                ActualDepUtc:    null,
                ActualArrUtc:    null,
                TerminalCode:    "T4",
                GateCode:        "D2",
                DelayReasonText: null,
                LastUpdatedUtc:  new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc)),

            // SR300 absent (QuickFlight only)
            // SR500 absent (neither provider — Unknown)
            // SR800 absent (QuickFlight only)
        };

    public Task<ProviderFlightStatusCandidate?> GetStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken)
    {
        if (!Stubs.TryGetValue(query.FlightNumber, out var raw))
            return Task.FromResult<ProviderFlightStatusCandidate?>(null);

        var unified = MapStatus(raw.FlightStatus);

        // delayReason only surfaces when status is Delayed (BR-RULE-032)
        var delayReason = unified == UnifiedFlightStatus.Delayed ? raw.DelayReasonText : null;

        var candidate = new ProviderFlightStatusCandidate(
            ProviderName:         ProviderName,
            FlightNumber:         query.FlightNumber,
            Date:                 query.Date.ToString("yyyy-MM-dd"),
            Status:               unified,
            LastUpdatedUtc:       raw.LastUpdatedUtc,
            ScheduledDepartureUtc: raw.SchedDepUtc,
            ScheduledArrivalUtc:  raw.SchedArrUtc,
            ActualDepartureUtc:   raw.ActualDepUtc,
            ActualArrivalUtc:     raw.ActualArrUtc,
            Terminal:             raw.TerminalCode,
            Gate:                 raw.GateCode,
            DelayReason:          delayReason,
            StatusRaw:            raw.FlightStatus);

        return Task.FromResult<ProviderFlightStatusCandidate?>(candidate);
    }

    /// <summary>Maps AeroTrack raw status vocabulary to unified enum (BR-RULE-004..009).</summary>
    private static UnifiedFlightStatus MapStatus(string rawStatus) =>
        rawStatus.ToUpperInvariant() switch
        {
            "ON_SCHEDULE" => UnifiedFlightStatus.OnTime,
            "DELAYED"     => UnifiedFlightStatus.Delayed,
            "CANCELED"    => UnifiedFlightStatus.Cancelled,
            "DIVERTED"    => UnifiedFlightStatus.Diverted,
            "UNKNOWN"     => UnifiedFlightStatus.Unknown,
            _             => UnifiedFlightStatus.Unknown  // BR-RULE-009: unrecognized -> Unknown
        };
}
