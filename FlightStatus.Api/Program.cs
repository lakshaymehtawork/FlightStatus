using System.Text.Json.Serialization;
using FlightStatus.Api.Application.Services;
using FlightStatus.Api.Contracts;
using FlightStatus.Api.Domain.Abstractions;
using FlightStatus.Api.Domain.Models;
using FlightStatus.Api.Infrastructure.Providers.AeroTrack;
using FlightStatus.Api.Infrastructure.Providers.QuickFlight;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── JSON: camelCase + enums as strings ───────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// ── DI registrations (architecture.md Section 5) ─────────────────────────────
builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackStubProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightStubProvider>();

// ── CORS: allow Angular dev server (ASSUMPTION-012) ──────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── OpenAPI / Scalar UI ───────────────────────────────────────────────────────
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Scalar UI at /scalar/v1
}

app.UseHttpsRedirection();

// ── Global exception handler → ApiError 500 ──────────────────────────────────
app.UseExceptionHandler(errorApp =>
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ApiError(
            "INTERNAL_ERROR",
            "An unexpected error occurred.",
            null,
            context.TraceIdentifier));
    }));

// ── Flight Status Endpoint ────────────────────────────────────────────────────
app.MapGet("/flights/status", async (
    string? flightNumber,
    string? date,
    IFlightStatusService flightStatusService,
    HttpContext httpContext,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation(
        "Request received: FlightNumber={FlightNumber} Date={Date}",
        flightNumber, date);

    // ── Input validation at HTTP boundary (BR-RULE-001, 002, 003) ────────────
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(flightNumber))
        errors.Add("flightNumber is required.");

    if (string.IsNullOrWhiteSpace(date))
        errors.Add("date is required.");

    DateOnly parsedDate = default;
    if (!string.IsNullOrWhiteSpace(date) &&
        !DateOnly.TryParseExact(date, "yyyy-MM-dd", out parsedDate))
        errors.Add("date must be in yyyy-MM-dd format.");

    if (errors.Count > 0)
        return Results.BadRequest(new ApiError(
            "VALIDATION_ERROR",
            "Request validation failed.",
            errors,
            httpContext.TraceIdentifier));

    var query = new FlightStatusQuery(flightNumber!, parsedDate);
    var result = await flightStatusService.GetFlightStatusAsync(query, cancellationToken);
    return Results.Ok(result);
})
.WithName("GetFlightStatus")
.WithSummary("Look up flight status by flight number and date.")
.WithDescription("Queries AeroTrack and QuickFlight stub providers concurrently and returns a unified FlightStatusResult.")
.Produces<FlightStatusResult>(StatusCodes.Status200OK)
.Produces<ApiError>(StatusCodes.Status400BadRequest)
.Produces<ApiError>(StatusCodes.Status500InternalServerError);

app.Run();

public partial class Program;

