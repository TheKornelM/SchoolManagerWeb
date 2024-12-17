using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Delete
{
    [Inject] private IDbContextFactory<SchoolManagerModel.Persistence.SchoolDbContext> DbFactory { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private Notifier Notifier { get; set; }

    private Class? currentClass { get; set; }

    [SupplyParameterFromQuery] private int Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        using var context = DbFactory.CreateDbContext();
        currentClass = await context.Classes.FirstOrDefaultAsync(m => m.Id == Id);

        if (currentClass is null)
        {
            NavigationManager.NavigateTo("notfound");
        }
    }

    private async Task DeleteClass()
    {
        if (currentClass is null)
        {
            NavigationManager.NavigateTo("notfound");
            return;
        }

        if (!await ClassManager.ClassExistsAsync(currentClass))
        {
            Notifier.ShowError("Class not found. It was probably already deleted from the database");
            return;
        }

        var subjectsTask = ClassManager.GetClassSubjectsAsync(currentClass);
        var studentsTask = ClassManager.GetClassStudentsAsync(currentClass);
        var classesTask = ClassManager.GetClassesAsync();

        await Task.WhenAll(subjectsTask, studentsTask, classesTask);

        var subjects = subjectsTask.Result;
        var classes = classesTask.Result;
        var students = studentsTask.Result;

        if (subjects.Count != 0 || students.Count != 0)
        {
            Notifier.ShowError("Cannot delete a class that has assigned students or subjects.");
            return;
        }

        using var context = DbFactory.CreateDbContext();
        context.Classes.Remove(currentClass!);
        await context.SaveChangesAsync();
        Notifier.ShowSuccess($"{currentClass.Name} was deleted successfully.");
        NavigationManager.NavigateTo("/classes");
    }
}