namespace FlightStatus.Api.Domain.Models;

/// <summary>Represents a flight status lookup request passed to providers and the service.</summary>
public record FlightStatusQuery(string FlightNumber, DateOnly Date);
