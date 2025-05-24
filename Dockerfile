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
COPY ["Api/KosmosERP.Api.csproj", "Api/"]
COPY ["Shared/KosmosERP.BusinessLayer/KosmosERP.BusinessLayer.csproj", "Shared/KosmosERP.BusinessLayer/"]
COPY ["Shared/KosmosERP.Models/KosmosERP.Models.csproj", "Shared/KosmosERP.Models/"]
COPY ["Shared/KosmosERP.Database/KosmosERP.Database.csproj", "Shared/KosmosERP.Database/"]
COPY ["Shared/KosmosERP.Module/KosmosERP.Module.csproj", "Shared/KosmosERP.Module/"]
RUN dotnet restore "./Api/KosmosERP.Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "./KosmosERP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KosmosERP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KosmosERP.Api.dll"]