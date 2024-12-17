using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Details
{
    private Class? currentClass;
    private List<User> Students;
    [SupplyParameterFromQuery] private int Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = DbFactory.CreateDbContext();
        currentClass = await context.Classes.FirstOrDefaultAsync(m => m.Id == Id);

        if (currentClass is null)
        {
            NavigationManager.NavigateTo("notfound");
            return;
        }


        var result = await ClassManager.GetClassStudentsAsync(currentClass);

        Students = result;
    }
}