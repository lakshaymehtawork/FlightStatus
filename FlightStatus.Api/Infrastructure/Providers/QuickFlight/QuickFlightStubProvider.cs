using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;

namespace FlightStatus.Api.Infrastructure.Providers.QuickFlight;

/// <summary>
/// Deterministic stub implementation of IFlightStatusProvider for QuickFlight.
/// QuickFlight returns minimal data — status and scheduled times only.
/// No terminal, gate, actual times, or delay reason (BR-RULE-031).
///
/// Stub dataset covers:
///   SR100 - OnTime;  both providers respond, AeroTrack has later timestamp (AeroTrack wins)
///   SR300 - Cancelled; QuickFlight only (AeroTrack absent)
///   SR400 - Diverted; both respond, same timestamp (AeroTrack wins tie-break)
///   SR600 - Delayed (LATE); both respond, QuickFlight has later timestamp (QuickFlight wins)
///   SR700 - Cancelled; both respond, same timestamp (AeroTrack wins tie-break)
///   SR800 - OnTime;  QuickFlight only (AeroTrack absent)
///   SR200, SR500 - absent (returns null) to support partial/total failure tests
/// </summary>
public sealed class QuickFlightStubProvider : IFlightStatusProvider
{
    public string ProviderName => "QuickFlight";

    // Keyed by flight number (case-insensitive); date is ignored for stub simplicity
    private static readonly IReadOnlyDictionary<string, QuickFlightRawResponse> Stubs =
        new Dictionary<string, QuickFlightRawResponse>(StringComparer.OrdinalIgnoreCase)
        {
            // SR100: OnTime — AeroTrack has later timestamp; AeroTrack wins
            ["SR100"] = new(
                Status:                  "ON_TIME",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20,  7, 30, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20,  9, 45, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20,  6,  0, 0, DateTimeKind.Utc)),

            // SR300: Cancelled — QuickFlight only (AeroTrack absent)
            ["SR300"] = new(
                Status:                  "CANCELLED",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20, 12,  0, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc)),

            // SR400: Diverted — same timestamp as AeroTrack; AeroTrack wins tie-break
            ["SR400"] = new(
                Status:                  "DIVERTED",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc)),

            // SR600: Delayed (LATE) — QuickFlight has later timestamp; QuickFlight wins
            ["SR600"] = new(
                Status:                  "LATE",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20,  6,  0, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20,  8,  0, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20, 12,  0, 0, DateTimeKind.Utc)),

            // SR700: Cancelled — same timestamp as AeroTrack; AeroTrack wins tie-break
            ["SR700"] = new(
                Status:                  "CANCELLED",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20, 14,  0, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20, 16,  0, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20, 10,  0, 0, DateTimeKind.Utc)),

            // SR800: OnTime — QuickFlight only (AeroTrack absent)
            ["SR800"] = new(
                Status:                  "ON_TIME",
                ScheduledDepartureUtc:   new DateTime(2026, 7, 20,  9,  0, 0, DateTimeKind.Utc),
                ScheduledArrivalUtc:     new DateTime(2026, 7, 20, 11,  0, 0, DateTimeKind.Utc),
                LastUpdatedUtc:          new DateTime(2026, 7, 20,  7,  0, 0, DateTimeKind.Utc)),

            // SR200 absent (AeroTrack only)
            // SR500 absent (neither provider — Unknown)
        };

    public Task<ProviderFlightStatusCandidate?> GetStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken)
    {
        if (!Stubs.TryGetValue(query.FlightNumber, out var raw))
            return Task.FromResult<ProviderFlightStatusCandidate?>(null);

        var unified = MapStatus(raw.Status);

        // BR-RULE-031: QuickFlight never populates terminal, gate, actual times, or delayReason
        var candidate = new ProviderFlightStatusCandidate(
            ProviderName:          ProviderName,
            FlightNumber:          query.FlightNumber,
            Date:                  query.Date.ToString("yyyy-MM-dd"),
            Status:                unified,
            LastUpdatedUtc:        raw.LastUpdatedUtc,
            ScheduledDepartureUtc: raw.ScheduledDepartureUtc,
            ScheduledArrivalUtc:   raw.ScheduledArrivalUtc,
            ActualDepartureUtc:    null,
            ActualArrivalUtc:      null,
            Terminal:              null,
            Gate:                  null,
            DelayReason:           null,
            StatusRaw:             raw.Status);

        return Task.FromResult<ProviderFlightStatusCandidate?>(candidate);
    }

    /// <summary>Maps QuickFlight raw status vocabulary to unified enum (BR-RULE-010..015).</summary>
    private static UnifiedFlightStatus MapStatus(string rawStatus) =>
        rawStatus.ToUpperInvariant() switch
        {
            "ON_TIME"   => UnifiedFlightStatus.OnTime,
            "LATE"      => UnifiedFlightStatus.Delayed,
            "CANCELLED" => UnifiedFlightStatus.Cancelled,
            "DIVERTED"  => UnifiedFlightStatus.Diverted,
            "UNKNOWN"   => UnifiedFlightStatus.Unknown,
            _           => UnifiedFlightStatus.Unknown   // BR-RULE-015: unrecognized -> Unknown
        };
}
