using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Utils;
using SchoolManagerModel.Validators;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Edit
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDbContextFactory<SchoolManagerModel.Persistence.SchoolDbContext> DbFactory { get; set; } = null!;
    [Inject] private Notifier Notifier { get; set; }

    [SupplyParameterFromQuery] private int Id { get; set; }

    [SupplyParameterFromForm] private Class? Class { get; set; }

    ClassValidator ClassValidator = new(UIResourceFactory.GetNewResource());

    protected override async Task OnInitializedAsync()
    {
        using var context = DbFactory.CreateDbContext();
        Class ??= await context.Classes.FirstOrDefaultAsync(m => m.Id == Id);

        if (Class is null)
        {
            NavigationManager.NavigateTo("notfound");
        }
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more information, see https://learn.microsoft.com/aspnet/core/blazor/forms/#mitigate-overposting-attacks.
    private async Task UpdateClass()
    {
        Class!.SchoolClass = Class!.SchoolClass.ToUpper();
        using var context = DbFactory.CreateDbContext();
        context.Attach(Class!).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
            Notifier.ShowSuccess("Class has been updated!");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClassExists(Class!.Id))
            {
                Notifier.ShowError("Class not found. It was probably deleted from the database.");
                NavigationManager.NavigateTo("notfound");
            }
            else
            {
                throw;
            }
        }

        NavigationManager.NavigateTo("/classes");
    }

    private bool ClassExists(int id)
    {
        using var context = DbFactory.CreateDbContext();
        return context.Classes.Any(e => e.Id == id);
    }
}