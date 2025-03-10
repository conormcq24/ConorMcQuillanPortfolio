# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image for compilation
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore dependencies with explicit NuGet settings
COPY ["ConorMcQuillanPortfolio.csproj", "./"]
RUN dotnet restore "./ConorMcQuillanPortfolio.csproj" --source https://api.nuget.org/v3/index.json --disable-parallel

# Create custom NuGet.config that ignores Visual Studio folders
RUN echo '<?xml version="1.0" encoding="utf-8"?>\n\
<configuration>\n\
  <packageSources>\n\
    <clear />\n\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />\n\
  </packageSources>\n\
  <fallbackPackageFolders>\n\
    <clear />\n\
  </fallbackPackageFolders>\n\
</configuration>' > nuget.config

# Copy the rest of the code and build with the custom config
COPY . .
RUN dotnet build "ConorMcQuillanPortfolio.csproj" -c $BUILD_CONFIGURATION -o /app/build --configfile ./nuget.config

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConorMcQuillanPortfolio.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --configfile ./nuget.config

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV GITHUB_REPO_ACCESS=""
ENTRYPOINT ["dotnet", "ConorMcQuillanPortfolio.dll"]