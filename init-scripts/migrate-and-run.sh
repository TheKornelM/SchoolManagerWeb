#!/bin/sh
set -e

# Check if a connection string is provided via environment variable
if [ -z "$ConnectionStrings__DefaultConnection" ]; then
  echo "Error: $ConnectionStrings__DefaultConnection environment variable is not set."
  exit 1
fi

echo "Waiting for PostgreSQL at $POSTGRES_HOST:$POSTGRES_PORT to be ready..."
RETRIES=30
until nc -z "$POSTGRES_HOST" "$POSTGRES_PORT" || [ $RETRIES -eq 0 ]; do
  echo "PostgreSQL is not ready yet. Retrying in 1 second... ($RETRIES retries left)"
  RETRIES=$((RETRIES - 1))
  sleep 1
done

if [ $RETRIES -eq 0 ]; then
  echo "Error: PostgreSQL is not ready after waiting."
  exit 1
fi

echo "Applying database migrations with the provided connection string.."
exec app/efbundle --connection "$ConnectionStrings__DefaultConnection"

echo "Starting the application...."
exec dotnet app/SchoolManagerWeb.dll
