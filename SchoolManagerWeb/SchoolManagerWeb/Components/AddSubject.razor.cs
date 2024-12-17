using Microsoft.AspNetCore.Components;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components;

public partial class AddSubject
{
    [Inject] public required Notifier Notifier { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Parameter] public required Class SelectedClass { get; set; } // Class passed as parameter

    private List<Teacher> teachers = [];

    // private string subjectName = string.Empty;
    private Subject newSubject = new Subject
    {
        Name = string.Empty,
        Teacher = null!,
        Class = new Class()
        {
            Year = 1,
            SchoolClass = string.Empty
        },
    };

    protected override async Task OnInitializedAsync()
    {
        // Fetch the list of teachers
        teachers = await TeacherManager.GetTeacherUsersAsync();

        // Initialize new Subject
        newSubject = new Subject
        {
            Class = SelectedClass,
            Name = string.Empty,
            Teacher = null!
        };
    }

    private async Task OnSubmit()
    {
        // Validation: Check for null or empty inputs
        if (string.IsNullOrWhiteSpace(newSubject.Name))
        {
            Notifier.ShowError("Subject name is required.");
            return;
        }

        // Check if the class exists
        var classExists = await ClassManager.ClassExistsAsync(SelectedClass);
        if (!classExists)
        {
            Notifier.ShowError("The selected class does not exist.");
            NavigationManager.NavigateTo("/classes");
            return;
        }

        try
        {
            await SubjectManager.AddSubjectAsync(newSubject);
            Notifier.ShowSuccess("Subject added successfully.");
            CloseDialog();
        }
        catch (Exception ex)
        {
            Notifier.ShowError($"Error saving subject: {ex.Message}");
        }
    }

    private void CloseDialog()
    {
        DialogService.Close();
    }
}