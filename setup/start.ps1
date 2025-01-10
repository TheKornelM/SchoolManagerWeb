Set-Location -Path ".."
mkdir init-scripts

Set-Location -Path "SchoolManagerWeb/SchoolManagerWeb"
mkdir certs

# Generate HTTPS development certificate and export it
dotnet dev-certs https -ep ".\certs\SchoolManagerWeb.pfx" -p "password"

# Trust the development certificate
dotnet dev-certs https --trust

# Generate the migrations.sql script
dotnet ef migrations script -o ../../init-scripts/migrations.sql --context SchoolDbContext

# Start containers
Set-Location -Path "../.."
docker-compose up