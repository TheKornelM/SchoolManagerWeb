Set-Location -Path "SchoolManagerWeb/SchoolManagerWeb"
mkdir certs

# Generate HTTPS development certificate and export it
dotnet dev-certs https -ep ".\certs\SchoolManagerWeb.pfx" -p "password"

# Trust the development certificate
dotnet dev-certs https --trust

# Start containers
Set-Location -Path "../.."
docker-compose up migrate
docker-compose up schoolmanagerweb