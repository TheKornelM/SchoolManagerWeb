using Microsoft.EntityFrameworkCore;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Persistence;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Index
{
    private SchoolDbContext context = default!;
    private List<Class>? Classes { get; set; }

    protected override async Task OnInitializedAsync()
    {
        context = DbFactory.CreateDbContext();
        Classes = await context.Classes
            .OrderBy(x => x.Year)
            .ThenBy(x => x.SchoolClass)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async ValueTask DisposeAsync() => await context.DisposeAsync();
}