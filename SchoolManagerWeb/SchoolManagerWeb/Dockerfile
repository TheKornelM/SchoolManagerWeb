# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["SchoolManagerWeb/SchoolManagerWeb/SchoolManagerWeb.csproj", "SchoolManagerWeb/SchoolManagerWeb/"]
COPY ["SchoolManagerModel/SchoolManagerModel/SchoolManagerModel.csproj", "SchoolManagerModel/SchoolManagerModel/"]
COPY ["SchoolManagerWeb/SchoolManagerWeb.Client/SchoolManagerWeb.Client.csproj", "SchoolManagerWeb/SchoolManagerWeb.Client/"]
RUN dotnet restore "./SchoolManagerWeb/SchoolManagerWeb/SchoolManagerWeb.csproj"
COPY . .

WORKDIR "/src/SchoolManagerWeb/SchoolManagerWeb"
RUN dotnet build "./SchoolManagerWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchoolManagerWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SchoolManagerWeb.dll"]
