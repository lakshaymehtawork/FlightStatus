using FlightStatus.Api.Domain.Enums;

namespace FlightStatus.Api.Domain.Models;

/// <summary>
/// Intermediate normalized candidate produced by each provider implementation.
/// The service selects from competing candidates using lastUpdatedUtc and tie-break rules.
/// </summary>
public record ProviderFlightStatusCandidate(
    string ProviderName,
    string FlightNumber,
    string Date,
    UnifiedFlightStatus Status,
    DateTime LastUpdatedUtc,         // Required; used for selection priority
    DateTime? ScheduledDepartureUtc,
    DateTime? ScheduledArrivalUtc,
    DateTime? ActualDepartureUtc,    // AeroTrack only
    DateTime? ActualArrivalUtc,      // AeroTrack only
    string? Terminal,                // AeroTrack only
    string? Gate,                    // AeroTrack only
    string? DelayReason,             // Delayed + provider supplies it
    string? StatusRaw                // Original provider vocabulary value for traceability
);
