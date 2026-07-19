namespace FlightStatus.Api.Domain.Enums;

/// <summary>Unified flight status enum shared across all providers and the API response.</summary>
public enum UnifiedFlightStatus
{
    /// <summary>Departing or arrived within 15 minutes of schedule.</summary>
    OnTime,

    /// <summary>Departure or arrival pushed beyond 15 minutes.</summary>
    Delayed,

    /// <summary>Flight will not operate.</summary>
    Cancelled,

    /// <summary>Flight landed at a different airport.</summary>
    Diverted,

    /// <summary>Provider returned no usable status.</summary>
    Unknown
}
