using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Models;

namespace FlightStatus.Tests.TestDoubles;

public sealed class ThrowingProviderStub : IFlightStatusProvider
{
    public ThrowingProviderStub(string providerName, string message = "Simulated provider failure")
    {
        ProviderName = providerName;
        Message = message;
    }

    public string ProviderName { get; }

    private string Message { get; }

    public Task<ProviderFlightStatusCandidate?> GetStatusAsync(FlightStatusQuery query, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(Message);
    }
}
