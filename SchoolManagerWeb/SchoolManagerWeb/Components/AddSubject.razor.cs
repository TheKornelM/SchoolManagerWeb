using Microsoft.AspNetCore.Components;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components;

public partial class AddSubject
{
    [Inject] public required Notifier Notifier { get; set; }
    [Parameter] public required Class SelectedClass { get; set; } // Class passed as parameter

    private List<Teacher> teachers = new();
    private Teacher selectedTeacher;
    private string subjectName = string.Empty;
    private Subject newSubject;

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
        if (string.IsNullOrWhiteSpace(subjectName))
        {
            Notifier.ShowError("Subject name is required.");
            return;
        }

        if (selectedTeacher == null)
        {
            Notifier.ShowError("Please select a teacher.");
            return;
        }

        // Check if the class exists
        var classExists = await ClassManager.ClassExistsAsync(SelectedClass);
        if (!classExists)
        {
            Notifier.ShowError("The selected class does not exist.");
            return;
        }

        // Save the subject
        newSubject.Name = subjectName;
        newSubject.Teacher = selectedTeacher;

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

    private void OnCancel()
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        DialogService.Close();
    }
}