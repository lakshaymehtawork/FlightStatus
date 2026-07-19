using FlightStatus.Api.Domain.Enums;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Infrastructure.Providers.QuickFlight;

namespace FlightStatus.Tests.Unit.Infrastructure;

public class QuickFlightStubProviderTests
{
    private static readonly DateOnly QueryDate = new(2026, 7, 20);

    [Fact]
    public async Task GetStatusAsync_SR100_MapsOnTime()
    {
        var provider = new QuickFlightStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR100", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.OnTime, result!.Status);
        Assert.Equal("QuickFlight", result.ProviderName);
        Assert.Null(result.Terminal);
        Assert.Null(result.Gate);
        Assert.Null(result.DelayReason);
    }

    [Fact]
    public async Task GetStatusAsync_SR600_MapsLateToDelayed()
    {
        var provider = new QuickFlightStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR600", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Delayed, result!.Status);
    }

    [Fact]
    public async Task GetStatusAsync_SR300_MapsCancelled()
    {
        var provider = new QuickFlightStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR300", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Cancelled, result!.Status);
    }

    [Fact]
    public async Task GetStatusAsync_SR400_MapsDiverted()
    {
        var provider = new QuickFlightStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR400", QueryDate), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(UnifiedFlightStatus.Diverted, result!.Status);
    }

    [Fact]
    public async Task GetStatusAsync_UnknownFlight_ReturnsNull()
    {
        var provider = new QuickFlightStubProvider();

        var result = await provider.GetStatusAsync(new FlightStatusQuery("SR999", QueryDate), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatusAsync_FlightNumberCaseInsensitive_ReturnsMatch()
    {
        var provider = new QuickFlightStubProvider();

        var lower = await provider.GetStatusAsync(new FlightStatusQuery("sr800", QueryDate), CancellationToken.None);
        var upper = await provider.GetStatusAsync(new FlightStatusQuery("SR800", QueryDate), CancellationToken.None);

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.Equal(upper!.Status, lower!.Status);
        Assert.Equal(upper.LastUpdatedUtc, lower.LastUpdatedUtc);
    }

    [Fact]
    public async Task GetStatusAsync_SameInputTwice_IsDeterministic()
    {
        var provider = new QuickFlightStubProvider();
        var query = new FlightStatusQuery("SR300", QueryDate);

        var first = await provider.GetStatusAsync(query, CancellationToken.None);
        var second = await provider.GetStatusAsync(query, CancellationToken.None);

        Assert.Equal(first, second);
    }
}
