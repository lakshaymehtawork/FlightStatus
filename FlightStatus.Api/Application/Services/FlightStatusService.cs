using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using System.Diagnostics;

namespace FlightStatus.Api.Application.Services;

/// <summary>
/// Orchestrates concurrent provider calls, applies selection and fallback rules per spec.md BR-RULE-021..028.
/// </summary>
public sealed class FlightStatusService : IFlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;
    private readonly ILogger<FlightStatusService> _logger;

    public FlightStatusService(
        IEnumerable<IFlightStatusProvider> providers,
        ILogger<FlightStatusService> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    public async Task<FlightStatusResult> GetFlightStatusAsync(
        FlightStatusQuery query,
        CancellationToken cancellationToken)
    {
        // BR-RULE-021: call both providers concurrently
        var tasks = _providers
            .Select(p => SafeGetStatusAsync(p, query, cancellationToken))
            .ToList();

        var results = await Task.WhenAll(tasks);

        var candidates = results
            .Where(c => c is not null)
            .Cast<ProviderFlightStatusCandidate>()
            .ToList();

        // BR-RULE-022: prefer later lastUpdatedUtc; BR-RULE-025: AeroTrack wins tie (alphabetical)
        var winner = candidates.Count switch
        {
            0 => null,
            1 => candidates[0],
            _ => candidates
                .OrderByDescending(c => c.LastUpdatedUtc)
                .ThenBy(c => c.ProviderName) // "AeroTrack" < "QuickFlight" alphabetically
                .First()
        };

        if (winner is null)
        {
            // BR-RULE-024, BR-RULE-028: return Unknown with message
            _logger.LogInformation(
                "Final result: Status=Unknown ProviderUsed=none FlightNumber={FlightNumber} Date={Date}",
                query.FlightNumber, query.Date);

            return new FlightStatusResult(
                query.FlightNumber,
                query.Date,
                UnifiedFlightStatus.Unknown,
                "No provider returned usable status for the requested flight and date.",
                null, null, null, null, null, null, null, null, null);
        }

        _logger.LogInformation(
            "Final result: ProviderUsed={ProviderUsed} Status={Status} FlightNumber={FlightNumber} Date={Date}",
            winner.ProviderName, winner.Status, query.FlightNumber, query.Date);

        return new FlightStatusResult(
            winner.FlightNumber,
            query.Date,
            winner.Status,
            $"Status provided by {winner.ProviderName}.",
            winner.ProviderName,
            winner.LastUpdatedUtc,
            winner.ScheduledDepartureUtc,
            winner.ScheduledArrivalUtc,
            winner.ActualDepartureUtc,
            winner.ActualArrivalUtc,
            winner.Terminal,
            winner.Gate,
            winner.DelayReason);
    }

    /// <summary>
    /// Calls a single provider and swallows exceptions per BR-RULE-026/027.
    /// Logs WARN on exception; logs INFO on successful result.
    /// </summary>
    private async Task<ProviderFlightStatusCandidate?> SafeGetStatusAsync(
        IFlightStatusProvider provider,
        FlightStatusQuery query,
        CancellationToken cancellationToken)
    {
        var requestId = Activity.Current?.Id ?? "n/a";

        try
        {
            var candidate = await provider.GetStatusAsync(query, cancellationToken);

            _logger.LogInformation(
                "Provider result: RequestId={RequestId} ProviderName={ProviderName} FlightNumber={FlightNumber} Date={Date} Status={Status}",
                requestId,
                provider.ProviderName,
                query.FlightNumber,
                query.Date,
                candidate?.Status.ToString() ?? "null");

            return candidate;
        }
        catch (Exception ex)
        {
            // BR-RULE-026: one provider exception must not cancel the other
            _logger.LogWarning(
                "Provider exception: RequestId={RequestId} ProviderName={ProviderName} ExceptionType={ExceptionType} Message={Message} FlightNumber={FlightNumber} Date={Date}",
                requestId, provider.ProviderName, ex.GetType().Name, ex.Message, query.FlightNumber, query.Date);

            return null;
        }
    }
}
