using FlightStatus.Api.Application.Services;
using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FlightStatus.Tests.Unit.Application;

public class FlightStatusServiceTests
{
    [Fact]
    public async Task GetFlightStatusAsync_BothProvidersRespond_PrefersHigherLastUpdatedUtc()
    {
        var query = new FlightStatusQuery("SR100", new DateOnly(2026, 7, 20));

        var older = Candidate("QuickFlight", UnifiedFlightStatus.OnTime, new DateTime(2026, 7, 20, 6, 0, 0, DateTimeKind.Utc));
        var newer = Candidate("AeroTrack", UnifiedFlightStatus.Delayed, new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));

        var service = BuildService(
            MockProvider("QuickFlight", older),
            MockProvider("AeroTrack", newer));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Equal("AeroTrack", result.ProviderUsed);
        Assert.Equal(UnifiedFlightStatus.Delayed, result.Status);
        Assert.Equal(newer.LastUpdatedUtc, result.LastUpdatedUtc);
    }

    [Fact]
    public async Task GetFlightStatusAsync_BothProvidersSameTimestamp_PrefersAeroTrackTieBreak()
    {
        var query = new FlightStatusQuery("SR400", new DateOnly(2026, 7, 20));
        var timestamp = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);

        var aero = Candidate("AeroTrack", UnifiedFlightStatus.Diverted, timestamp);
        var quick = Candidate("QuickFlight", UnifiedFlightStatus.Cancelled, timestamp);

        var service = BuildService(
            MockProvider("QuickFlight", quick),
            MockProvider("AeroTrack", aero));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Equal("AeroTrack", result.ProviderUsed);
        Assert.Equal(UnifiedFlightStatus.Diverted, result.Status);
    }

    [Fact]
    public async Task GetFlightStatusAsync_OnlyOneProviderResponds_UsesThatResult()
    {
        var query = new FlightStatusQuery("SR300", new DateOnly(2026, 7, 20));

        var quick = Candidate("QuickFlight", UnifiedFlightStatus.Cancelled, new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));

        var service = BuildService(
            MockProvider("QuickFlight", quick),
            MockProvider("AeroTrack", null));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Equal("QuickFlight", result.ProviderUsed);
        Assert.Equal(UnifiedFlightStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task GetFlightStatusAsync_NoProvidersRespond_ReturnsUnknownWithMessage()
    {
        var query = new FlightStatusQuery("SR500", new DateOnly(2026, 7, 20));

        var service = BuildService(
            MockProvider("QuickFlight", null),
            MockProvider("AeroTrack", null));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Null(result.ProviderUsed);
        Assert.Equal(UnifiedFlightStatus.Unknown, result.Status);
        Assert.Contains("No provider returned usable status", result.Message);
    }

    [Fact]
    public async Task GetFlightStatusAsync_OneProviderThrows_UsesOtherProviderResult()
    {
        var query = new FlightStatusQuery("SR200", new DateOnly(2026, 7, 20));

        var aero = Candidate("AeroTrack", UnifiedFlightStatus.Delayed, new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc));

        var service = BuildService(
            new ThrowingProviderStub("QuickFlight"),
            MockProvider("AeroTrack", aero));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Equal("AeroTrack", result.ProviderUsed);
        Assert.Equal(UnifiedFlightStatus.Delayed, result.Status);
    }

    [Fact]
    public async Task GetFlightStatusAsync_AllProvidersThrow_ReturnsUnknown()
    {
        var query = new FlightStatusQuery("SR500", new DateOnly(2026, 7, 20));

        var service = BuildService(
            new ThrowingProviderStub("QuickFlight"),
            new ThrowingProviderStub("AeroTrack"));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.Equal(UnifiedFlightStatus.Unknown, result.Status);
        Assert.Null(result.ProviderUsed);
    }

    [Fact]
    public async Task GetFlightStatusAsync_WinnerHasNullDelayReason_MessageStillNonEmpty()
    {
        var query = new FlightStatusQuery("SR100", new DateOnly(2026, 7, 20));
        var aero = Candidate("AeroTrack", UnifiedFlightStatus.OnTime, new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));

        var service = BuildService(MockProvider("AeroTrack", aero));

        var result = await service.GetFlightStatusAsync(query, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    private static FlightStatusService BuildService(params IFlightStatusProvider[] providers)
    {
        return new FlightStatusService(providers, NullLogger<FlightStatusService>.Instance);
    }

    private static IFlightStatusProvider MockProvider(string providerName, ProviderFlightStatusCandidate? candidate)
    {
        var mock = new Mock<IFlightStatusProvider>(MockBehavior.Strict);
        mock.SetupGet(x => x.ProviderName).Returns(providerName);
        mock.Setup(x => x.GetStatusAsync(It.IsAny<FlightStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidate);
        return mock.Object;
    }

    private static ProviderFlightStatusCandidate Candidate(
        string provider,
        UnifiedFlightStatus status,
        DateTime updatedUtc)
    {
        return new ProviderFlightStatusCandidate(
            provider,
            "SRX",
            "2026-07-20",
            status,
            updatedUtc,
            null,
            null,
            null,
            null,
            provider == "AeroTrack" ? "T1" : null,
            provider == "AeroTrack" ? "A1" : null,
            status == UnifiedFlightStatus.Delayed ? "Delay" : null,
            status.ToString().ToUpperInvariant());
    }
}
