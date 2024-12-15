using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SchoolManagerModel.Entities;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Delete
{
    [Inject] IDbContextFactory<SchoolManagerModel.Persistence.SchoolDbContext> DbFactory { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }
    [Inject] public required NotificationService NotificationService { get; set; }

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

        var subjects = await ClassManager.GetClassSubjectsAsync(currentClass);
        var students = await ClassManager.GetClassStudentsAsync(currentClass);

        if (subjects.Count != 0 || students.Count != 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "Cannot delete a class that has assigned students or subjects",
                Duration = 4000,
                Style = "word-break:break-word"
            });
            return;
        }

        using var context = DbFactory.CreateDbContext();
        context.Classes.Remove(currentClass!);
        await context.SaveChangesAsync();
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = $"{currentClass.Name} was deleted successfully",
            Duration = 4000,
            Style = "word-break:break-word"
        });

        NavigationManager.NavigateTo("/classes");
    }
}