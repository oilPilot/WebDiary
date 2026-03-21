# Multi-stage Dockerfile for WebDiary Frontend (Blazor Server)
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

# Copy solution and project files
COPY ["WebDiary.sln", "."]
COPY ["WebDiary.Frontend/WebDiary.Frontend.csproj", "WebDiary.Frontend/"]

# Restore dependencies
RUN dotnet restore "WebDiary.sln"

# Copy the entire solution
COPY . .

# Build the application
RUN dotnet build "WebDiary.Frontend/WebDiary.Frontend.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "WebDiary.Frontend/WebDiary.Frontend.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Create non-root user for security
RUN useradd -m -u 1001 appuser && \
    chown -R appuser:appuser /app

USER appuser

# Copy published application from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5282 \
    ASPNETCORE_HTTP_PORTS=5282

# Expose port
EXPOSE 5282

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet --version || exit 1

# Run the application
ENTRYPOINT ["dotnet", "WebDiary.Frontend.dll"]
