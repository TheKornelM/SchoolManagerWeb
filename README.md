# HTTPS connection

You need to create HTTPS certificate, move it to SchoolManagerWeb\certs.

Check https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide for more informations about certificate creation.

# Migration

Create a new migration from project root folder:

`dotnet ef migrations add InitialCreate --project SchoolManagerModel/SchoolManagerModel --startup-project SchoolManagerWeb/SchoolManagerWeb --context SchoolDbContext`

# Environment file

This section describes the environment variables used to configure SchoolManager in a Dockerized environment.

## Application-Specific Environment Variables

### ASP.NET Core Configuration
- **`ASPNETCORE_Kestrel__Certificates__Default__Path`**
  - Path to the self-signed SSL certificate used by Kestrel.
  - **Example:** `/https/SchoolManagerWeb.pfx`

- **`ASPNETCORE_Kestrel__Certificates__Default__Password`**
  - Password for the SSL certificate.
  - **Example:** `password`

- **`ASPNETCORE_URLS`**
  - Specifies the URLs that the application will listen on.
  - **Example:** `http://+:5000;https://+:5001`

- **`ASPNETCORE_Environment`**
  - Sets the environment for the application (e.g., Development, Staging, Production).
  - **Example:** `Production`

### Database Connection String
- **`ConnectionStrings__DefaultConnection`**
  - Connection string for the PostgreSQL database.
  - **Example:** `"Host=postgres:5432;Database=postgres;Username=postgres;Password=password"`

## PostgreSQL Database Configuration

- **`POSTGRES_USER`**
  - Username for the PostgreSQL database.
  - **Example:** `postgres`

- **`POSTGRES_PASSWORD`**
  - Password for the PostgreSQL database user.
  - **Example:** `password`

- **`POSTGRES_DB`**
  - Name of the default PostgreSQL database to be created.
  - **Example:** `postgres`

---

## Usage

1. Save the above environment variables in a `.env` file in the root directory of your project.
2. Ensure that the `.env` file is included in the Docker Compose configuration.
3. Update the values of the variables to match your production or development environment settings.

There is an example environment file in the root directory: `.env.example`