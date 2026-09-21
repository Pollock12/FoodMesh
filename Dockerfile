# ==========================================
# Multi-Stage Dockerfile for FoodMesh API (.NET 8)
# ==========================================

# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project definition files for layer caching
COPY FoodMesh.sln ./
COPY src/Shared/FoodMesh.Shared.csproj src/Shared/
COPY src/Domain/FoodMesh.Domain.csproj src/Domain/
COPY src/Application/FoodMesh.Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure/FoodMesh.Infrastructure.csproj src/Infrastructure/Infrastructure/
COPY src/Read/FoodMesh.Read.csproj src/Read/
COPY src/Read/BusinessApiService/FoodMesh.BusinessApiService.csproj src/Read/BusinessApiService/

# Restore NuGet packages
RUN dotnet restore src/Read/BusinessApiService/FoodMesh.BusinessApiService.csproj

# Copy remaining source files
COPY src/ ./src/

# Publish the BusinessApiService
WORKDIR /src/src/Read/BusinessApiService
RUN dotnet publish FoodMesh.BusinessApiService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set default port (8080) - overridden dynamically if host injects $PORT
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FoodMesh.BusinessApiService.dll"]
