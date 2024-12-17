using Microsoft.AspNetCore.Components;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;

namespace SchoolManagerWeb.Components;

public partial class AddSubject
{
    [Parameter] public Class SelectedClass { get; set; } // Class passed as parameter
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
            await ShowErrorMessage("Subject name is required.");
            return;
        }

        if (selectedTeacher == null)
        {
            await ShowErrorMessage("Please select a teacher.");
            return;
        }

        // Check if the class exists
        var classExists = await ClassManager.ClassExistsAsync(SelectedClass);
        if (!classExists)
        {
            await ShowErrorMessage("The selected class does not exist.");
            return;
        }

        // Save the subject
        newSubject.Name = subjectName;
        newSubject.Teacher = selectedTeacher;

        try
        {
            await SubjectManager.AddSubjectAsync(newSubject);
            await ShowSuccessMessage("Subject added successfully.");
            CloseDialog();
        }
        catch (Exception ex)
        {
            await ShowErrorMessage($"Error saving subject: {ex.Message}");
        }
    }

    private void OnCancel()
    {
        CloseDialog();
    }

    private async Task ShowErrorMessage(string message)
    {
        await DialogService.Alert(message, "Error", new AlertOptions() { Width = "400px", Style = "color:red;" });
    }

    private async Task ShowSuccessMessage(string message)
    {
        await DialogService.Alert(message, "Success", new AlertOptions() { Width = "400px", Style = "color:green;" });
    }

    private void CloseDialog()
    {
        DialogService.Close();
    }
}