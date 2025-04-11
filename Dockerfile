# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api/Prometheus.Api/Prometheus.Api.csproj", "Api/Prometheus.Api/"]
COPY ["Shared/Prometheus.BusinessLayer/Prometheus.BusinessLayer.csproj", "Shared/Prometheus.BusinessLayer/"]
COPY ["Shared/Prometheus.Models/Prometheus.Models.csproj", "Shared/Prometheus.Models/"]
COPY ["Shared/Prometheus.Database/Prometheus.Database.csproj", "Shared/Prometheus.Database/"]
COPY ["Shared/Prometheus.Module/Prometheus.Module.csproj", "Shared/Prometheus.Module/"]
RUN dotnet restore "./Api/Prometheus.Api/Prometheus.Api.csproj"
COPY . .
WORKDIR "/src/Api/Prometheus.Api"
RUN dotnet build "./Prometheus.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Prometheus.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Prometheus.Api.dll"]