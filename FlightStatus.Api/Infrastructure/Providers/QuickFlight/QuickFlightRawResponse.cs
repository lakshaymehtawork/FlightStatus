namespace FlightStatus.Api.Infrastructure.Providers.QuickFlight;

/// <summary>
/// Represents the minimal, provider-specific raw response schema from QuickFlight.
/// QuickFlight returns only flight status and scheduled times — no gate, terminal, or delay reason.
/// </summary>
public record QuickFlightRawResponse(
    string Status,
    DateTime? ScheduledDepartureUtc,
    DateTime? ScheduledArrivalUtc,
    DateTime LastUpdatedUtc
);
