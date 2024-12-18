using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components;

public partial class AddSubjectMark
{
    [Inject] public required Notifier Notifier { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }

    [Parameter] public required Subject Subject { get; set; }
    [Parameter] public required Student Student { get; set; }

    public int Mark { get; set; } = 1;
    private string Notes { get; set; } = string.Empty;

    private async Task OnSubmit()
    {
        var mark = new Mark
        {
            Grade = Mark,
            Student = Student,
            Subject = Subject,
            Teacher = Subject.Teacher,
            SubmitDate = DateTime.UtcNow,
            Notes = Notes
        };

        try
        {
            await SubjectManager.AddSubjectMarkAsync(mark);
            Notifier.ShowSuccess("Mark added successfully.");
            CloseDialog();
        }
        catch (Exception ex)
        {
            Notifier.ShowError($"Error during saving mark: {ex.Message}");
        }
    }

    private void CloseDialog()
    {
        DialogService.Close();
    }
}