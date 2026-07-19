using FlightStatus.Api.Domain.Models;

namespace FlightStatus.Api.Domain.Abstractions;

/// <summary>
/// Strategy contract for flight data providers.
/// Each implementation adapts a single provider's schema to the internal candidate model.
/// Injected as IEnumerable&lt;IFlightStatusProvider&gt; into the service — never referenced directly.
/// </summary>
public interface IFlightStatusProvider
{
    /// <summary>Stable logical name of this provider (e.g. "AeroTrack", "QuickFlight").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Retrieves a normalized candidate for the given query.
    /// Returns null when the provider has no stub data for the flight/date combination.
    /// Throws on exceptional failure (caught by the aggregation service).
    /// </summary>
    Task<ProviderFlightStatusCandidate?> GetStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken);
}
