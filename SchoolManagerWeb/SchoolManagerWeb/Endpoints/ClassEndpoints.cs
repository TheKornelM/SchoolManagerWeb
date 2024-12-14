using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Persistence;

namespace SchoolManagerWeb.Endpoints;

public static class ClassEndpoints
{
    public static void MapClassEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/class").WithTags(nameof(Class));

        group.MapGet("/", async (SchoolDbContext db) => { return await db.Classes.ToListAsync(); })
            .WithName("GetAllClasses")
            .WithOpenApi()
            .RequireAuthorization("RequireAdminRole");

        group.MapGet("/{id}", async Task<Results<Ok<Class>, NotFound>> (int id, SchoolDbContext db) =>
            {
                return await db.Classes.AsNoTracking()
                        .FirstOrDefaultAsync(model => model.Id == id)
                    is Class model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
            })
            .WithName("GetClassById")
            .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Class @class, SchoolDbContext db) =>
            {
                // Put class name validator here
                var affected = await db.Classes
                    .Where(model => model.Id == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.Name, @class.Name)
                    );
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("UpdateClass")
            .WithOpenApi()
            .RequireAuthorization("RequireAdminRole");

        group.MapPost("/", async (Class @class, SchoolDbContext db) =>
            {
                db.Classes.Add(@class);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Class/{@class.Id}", @class);
            })
            .WithName("CreateClass")
            .WithOpenApi()
            .RequireAuthorization("RequireAdminRole");

        /*group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, SchoolDbContext db) =>
            {
                var affected = await db.Classes
                    .Where(model => model.Id == id)
                    .ExecuteDeleteAsync();
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("DeleteClass")
            .WithOpenApi();*/
    }
}