# TailorBlend AI Consultant - Frontend Dockerfile
# Blazor Server (.NET 8)
# Optimized for production deployment (fly.io, Railway, Google Cloud Run, etc.)

# ============================================================================
# Stage 1: Build
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Install Node.js and npm (required for Tailwind CSS build)
RUN apt-get update && apt-get install -y nodejs npm && apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy project file
COPY BlazorConsultant/BlazorConsultant.csproj ./BlazorConsultant/
RUN dotnet restore BlazorConsultant/BlazorConsultant.csproj

# Copy source code
COPY BlazorConsultant/ ./BlazorConsultant/

# Build and publish
WORKDIR /src/BlazorConsultant
RUN dotnet publish -c Release -o /app/publish

# ============================================================================
# Stage 2: Runtime
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://127.0.0.1:8080/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "BlazorConsultant.dll"]
