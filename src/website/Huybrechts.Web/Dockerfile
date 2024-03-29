#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/website/Huybrechts.Web/Huybrechts.Web.csproj", "src/website/Huybrechts.Web/"]
RUN dotnet restore "./src/website/Huybrechts.Web/./Huybrechts.Web.csproj"
COPY . .
WORKDIR "/src/src/website/Huybrechts.Web"
RUN dotnet build "./Huybrechts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Huybrechts.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_CLI_TELEMETRY_OPTOUT '1'
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Huybrechts.Web.dll"]