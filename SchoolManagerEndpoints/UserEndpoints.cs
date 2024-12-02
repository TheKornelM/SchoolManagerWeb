using Microsoft.AspNetCore.Builder;

namespace SchoolManagerEndpoints;

public static class UserEndpoints
{
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/hi", () => " hello");
        return app;
    }
}
