using Microsoft.AspNetCore.Components;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Utils;
using SchoolManagerModel.Validators;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Create
{
    [Inject] Notifier _notifier { get; set; }
    [Inject] ClassManager _classManager { get; set; }

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

        if (await ClassManager.ClassExistsAsync(Class))
        {
            _notifier.ShowError($"{Class.Name} already exists!");
            return;
        }

        await ClassManager.AddClassAsync(Class);
        _notifier.ShowSuccess($"{Class.Name} added successfully!");
        NavigationManager.NavigateTo("/classes");
    }
}