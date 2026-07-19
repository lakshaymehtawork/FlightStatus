using FlightStatus.Api.Domain.Models;

namespace FlightStatus.Api.Domain.Abstractions;

/// <summary>
/// Application service contract for flight status aggregation.
/// Coordinates provider calls, applies selection and fallback rules, and returns a unified result.
/// </summary>
public interface IFlightStatusService
{
    /// <summary>
    /// Queries all registered providers concurrently and returns a normalized FlightStatusResult.
    /// Always returns a result — Unknown when no provider has data.
    /// </summary>
    Task<FlightStatusResult> GetFlightStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken);
}
