# Base runtime image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Build stage for compiling the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SchoolManagerAPI/SchoolManagerAPI.csproj", "SchoolManagerAPI/"]
RUN dotnet restore "./SchoolManagerAPI/SchoolManagerAPI.csproj"
COPY . .
WORKDIR "/src/SchoolManagerAPI"
RUN dotnet build "./SchoolManagerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchoolManagerAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolManagerAPI.dll"]