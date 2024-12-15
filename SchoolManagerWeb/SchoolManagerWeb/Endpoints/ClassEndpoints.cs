using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Managers;
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

        group.MapGet("/{id}/subjects",
                async Task<Results<Ok<List<GetSubjectDto>>, NotFound, BadRequest>> (int id,
                    ClassManager classManager) =>
                {
                    var classes = await classManager.GetClassesAsync();

                    var @class = classes.FirstOrDefault(x => x.Id == id);

                    if (@class == null)
                    {
                        return TypedResults.NotFound();
                    }

                    var subjects = await classManager.GetClassSubjectsAsync(@class);

                    return TypedResults.Ok(subjects.Select(x => new GetSubjectDto(x.Id, x.Name)).ToList());
                })
            .WithName("GetClassSubjects")
            .WithOpenApi()
            .RequireAuthorization("RequireAdminRole");

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