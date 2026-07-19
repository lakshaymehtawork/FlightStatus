using FlightStatus.Api.Domain.Enums;

namespace FlightStatus.Api.Domain.Models;

/// <summary>
/// The normalized, provider-agnostic flight status result returned by the API.
/// AeroTrack-only fields (Terminal, Gate, ActualDepartureUtc, ActualArrivalUtc, DelayReason)
/// are null when the winning provider is QuickFlight or when the field is not applicable.
/// </summary>
public record FlightStatusResult(
    string FlightNumber,
    DateOnly Date,
    UnifiedFlightStatus Status,
    string Message,
    string? ProviderUsed,
    DateTime? LastUpdatedUtc,
    DateTime? ScheduledDepartureUtc,
    DateTime? ScheduledArrivalUtc,
    DateTime? ActualDepartureUtc,    // AeroTrack conditional
    DateTime? ActualArrivalUtc,      // AeroTrack conditional
    string? Terminal,                // AeroTrack conditional
    string? Gate,                    // AeroTrack conditional
    string? DelayReason              // Only when Delayed and provider supplies it
);
