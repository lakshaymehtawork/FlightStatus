using System.Net;
using System.Net.Http.Json;
using FlightStatus.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FlightStatus.Tests.Integration;

public class FlightStatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FlightStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetFlightStatus_ValidRequest_Returns200WithExpectedBody()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR100&date=20-07-2026");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.NotNull(body);
        Assert.Equal("SR100", body!["flightNumber"].ToString());
        Assert.Equal("OnTime", body["status"].ToString());
        Assert.Equal("AeroTrack", body["providerUsed"].ToString());
    }

    [Fact]
    public async Task GetFlightStatus_MissingFlightNumber_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?date=20-07-2026");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("VALIDATION_ERROR", error!.Code);
        Assert.Contains("flightNumber is required.", error.Details!);
    }

    [Fact]
    public async Task GetFlightStatus_InvalidFlightNumberCharacters_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR%26100&date=20-07-2026");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("VALIDATION_ERROR", error!.Code);
        Assert.Contains("flightNumber must contain only letters and digits.", error.Details!);
    }

    [Fact]
    public async Task GetFlightStatus_MissingDate_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("VALIDATION_ERROR", error!.Code);
        Assert.Contains("date is required.", error.Details!);
    }

    [Fact]
    public async Task GetFlightStatus_InvalidDateFormat_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR100&date=2026/07/20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("VALIDATION_ERROR", error!.Code);
        Assert.Contains("date must be in DD-MM-YYYY format.", error.Details!);
    }

    [Fact]
    public async Task GetFlightStatus_DateBeforeSupportedRange_Returns400()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR100&date=20-03-1000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error);
        Assert.Equal("VALIDATION_ERROR", error!.Code);
        Assert.Contains("date must be on or after 01-01-1900.", error.Details!);
    }

    [Fact]
    public async Task GetFlightStatus_NoProviderData_Returns200WithUnknown()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR500&date=20-07-2026");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.NotNull(body);
        Assert.Equal("Unknown", body!["status"].ToString());
        Assert.Equal("", body["providerUsed"]?.ToString() ?? "");
    }

    [Fact]
    public async Task HealthCheck_Returns200WithHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(body);
        Assert.Equal("Healthy", body!["status"]);
    }

    [Fact]
    public async Task GetFlightStatus_RequestedDate_AnchorsReturnedTimestampsToSearchDate()
    {
        var response = await _client.GetAsync("/flights/status?flightNumber=SR100&date=11-11-2000");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.NotNull(body);
        Assert.Equal("2000-11-11", body!["date"].ToString());
        Assert.StartsWith("2000-11-11", body["scheduledDepartureUtc"].ToString());
        Assert.StartsWith("2000-11-11", body["lastUpdatedUtc"].ToString());
    }
}
