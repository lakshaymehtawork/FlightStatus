FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore first for better layer caching
COPY FlightStatus.Api/FlightStatus.Api.csproj FlightStatus.Api/
RUN dotnet restore FlightStatus.Api/FlightStatus.Api.csproj

# Copy source and publish
COPY FlightStatus.Api/ FlightStatus.Api/
RUN dotnet publish FlightStatus.Api/FlightStatus.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects PORT dynamically
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet FlightStatus.Api.dll"]
