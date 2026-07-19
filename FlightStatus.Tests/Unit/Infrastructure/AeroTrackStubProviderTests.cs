using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Infrastructure.Providers.AeroTrack;

namespace FlightStatus.Tests.Unit.Infrastructure;

public class AeroTrackStubProviderTests
{
    private static readonly DateOnly QueryDate = new(2026, 7, 20);

    [Fact]
    public async Task GetStatusAsync_SR100_MapsOnScheduleToOnTime()
    {
        var provider = new AeroTrackStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR100", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.OnTime, result!.Status);
        Assert.Equal("AeroTrack", result.ProviderName);
        Assert.Equal("T1", result.Terminal);
        Assert.Equal("A1", result.Gate);
        Assert.Null(result.DelayReason);
    }

    [Fact]
    public async Task GetStatusAsync_SR200_MapsDelayedAndKeepsDelayReason()
    {
        var provider = new AeroTrackStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR200", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Delayed, result!.Status);
        Assert.Equal("Crew rotation delay", result.DelayReason);
    }

    [Fact]
    public async Task GetStatusAsync_SR400_MapsDiverted()
    {
        var provider = new AeroTrackStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR400", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Diverted, result!.Status);
    }

    [Fact]
    public async Task GetStatusAsync_SR700_MapsCanceledToCancelledAndClearsDelayReason()
    {
        var provider = new AeroTrackStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR700", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Cancelled, result!.Status);
        Assert.Null(result.DelayReason);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownFlight_ReturnsNull()
    {
        var provider = new AeroTrackStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR999", QueryDate), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatusAsync_FlightNumberCaseInsensitive_ReturnsMatch()
    {
        var provider = new AeroTrackStubProvider();

        var lower = await provider.GetStatusAsync(new FlightStatusQuery("sr100", QueryDate), CancellationToken.None);
        var upper = await provider.GetStatusAsync(new FlightStatusQuery("SR100", QueryDate), CancellationToken.None);

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.Equal(upper!.Status, lower!.Status);
        Assert.Equal(upper.LastUpdatedUtc, lower.LastUpdatedUtc);
    }

    [Fact]
    public async Task GetStatusAsync_SameInputTwice_IsDeterministic()
    {
        var provider = new AeroTrackStubProvider();
        var query = new FlightStatusQuery("SR200", QueryDate);

        var first = await provider.GetStatusAsync(query, CancellationToken.None);
        var second = await provider.GetStatusAsync(query, CancellationToken.None);

        Assert.Equal(first, second);
    }
}
