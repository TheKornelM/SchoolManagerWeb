# HTTPS connection

You need to create HTTPS certificate, move it to SchoolManagerWeb\certs.

Check https://learn.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide for more informations about certificate creation.

# Migration

Create a new migration from project root folder:

`dotnet ef migrations add InitialCreate --project SchoolManagerModel/SchoolManagerModel --startup-project SchoolManagerWeb/SchoolManagerWeb --context SchoolDbContext`
