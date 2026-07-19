namespace FlightStatus.Api.Contracts;

/// <summary>Standard error response payload for 4xx and 5xx HTTP responses.</summary>
public record ApiError(
    string Code,
    string Message,
    IReadOnlyList<string>? Details = null,
    string? RequestId = null
);
