# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["SchoolManagerWeb/SchoolManagerWeb/SchoolManagerWeb.csproj", "SchoolManagerWeb/SchoolManagerWeb/"]
COPY ["SchoolManagerModel/SchoolManagerModel/SchoolManagerModel.csproj", "SchoolManagerModel/SchoolManagerModel/"]
COPY ["SchoolManagerWeb/SchoolManagerWeb.Client/SchoolManagerWeb.Client.csproj", "SchoolManagerWeb/SchoolManagerWeb.Client/"]
RUN dotnet restore "./SchoolManagerWeb/SchoolManagerWeb/SchoolManagerWeb.csproj"
COPY . .

# Install dotnet-ef in the build stage
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR "/src/SchoolManagerWeb/SchoolManagerWeb"
RUN dotnet build "./SchoolManagerWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet ef migrations bundle --context SchoolDbContext --verbose --target-runtime linux-x64 --self-contained -o /app/efbundle

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchoolManagerWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app from the publish stage
COPY --from=publish /app/publish .

# Copy the efbundle
COPY --from=build /app/efbundle /app/efbundle

# Ensure the efbundle is executable
RUN chmod +x /app/efbundle

# Add an entrypoint script to handle migrations before starting the app
WORKDIR /
RUN apt-get update && apt-get install -y netcat-openbsd
COPY ["init-scripts/migrate-and-run.sh", "/"]
RUN chmod +x ./migrate-and-run.sh

ENTRYPOINT ["sh", "./migrate-and-run.sh"]
