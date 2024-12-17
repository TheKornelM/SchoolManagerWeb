using Microsoft.AspNetCore.Components;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Utils;
using SchoolManagerModel.Validators;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Create
{
    [SupplyParameterFromForm]
    private Class? Class { get; set; } = new()
    {
        Year = 1,
        SchoolClass = "A"
    };

    private ClassValidator ClassValidator { get; set; } = new(UIResourceFactory.GetNewResource());

    private async Task AddClass()
    {
        if (Class == null)
        {
            NavigationManager.NavigateTo("/notfound");
            return;
        }

        Class.SchoolClass = Class.SchoolClass.ToUpper();
        await using var context = DbFactory.CreateDbContext();
        context.Classes.Add(Class);
        await context.SaveChangesAsync();
        NavigationManager.NavigateTo("/classes");
    }
}