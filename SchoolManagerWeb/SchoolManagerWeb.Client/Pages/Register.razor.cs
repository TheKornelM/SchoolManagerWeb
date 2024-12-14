using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Radzen;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;

namespace SchoolManagerWeb.Client.Pages;

public partial class Register
{
    private IEnumerable<IdentityError>? identityErrors;

    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required NotificationService NotificationService { get; set; }

    [SupplyParameterFromForm] private UserRegistrationDto Input { get; set; } = new();

    private List<Class> Classes { get; set; } = [];
    private List<SelectSubjectDto> Subjects { get; set; } = [];
    private string? Message { get; set; }

    private bool ClassesAreLoading { get; set; } = false;

    private List<int> SelectedSubjects => Subjects
        .Where(x => x.IsSelected)
        .Select(s => s.Id)
        .ToList();

    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //Classes = await ClassManager.GetClassesAsync();
        AttachEventHandlers();
    }

    private async Task RegisterUser(EditContext editContext)
    {
        /*if (Input.Role == "Student" && string.IsNullOrEmpty(SelectedClassId))
        {
            identityErrors = new[] { new IdentityError { Description = "Students must select a class." } };
            Message = identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";
            return;
        }*/

        // API call:
        //var client = HttpClientFactory.CreateClient("ServerAPI");
        try
        {
            if (Input.Role == "Student")
            {
                Input.AssignedSubjects = SelectedSubjects;
            }

            var response = await HttpClient.PostAsJsonAsync("user", Input);

            // Check if the response indicates success
            if (response.IsSuccessStatusCode)
            {
                // Read and display the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                //essage = $"Success: {responseContent}";
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "You added user successfully",
                    Duration = 4000,
                    Style = "word-break:break-word"
                });
                Input = new UserRegistrationDto();
                AttachEventHandlers();
            }
            else
            {
                // Handle non-success responses
                var errorContent = await response.Content.ReadFromJsonAsync<IdentityError[]>() ?? [];
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Failure",
                    Detail = $"Error during user adding!\n{string.Join("\n", errorContent.Select(x => x.Description))}",
                    Duration = 4000,
                    Style = "word-break:break-word"
                });
                //Message = $"Error: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the API call
            Message = $"Exception: {ex.Message}";
        }

        Console.WriteLine(Message); // Log the message for debugging
    }

    private async Task RoleChanged()
    {
        if (string.IsNullOrEmpty(Input.Role)) return;

        if (Input.Role == "Student")
        {
            Input.AssignedClassId = null;
            Subjects.Clear();
            ClassesAreLoading = true;
            Classes = await GetClassesAsync();
            await Task.Delay(2000);
            ClassesAreLoading = false;
            StateHasChanged();
        }
        else
        {
            Input.AssignedClassId = null;
            Subjects.Clear();
            Classes.Clear();
        }
    }

    private async Task ClassChanged()
    {
        if (Input.AssignedClassId == null)
        {
            Subjects.Clear();
            Console.WriteLine("Subject list cleared");
            return;
        }

        Subjects = await GetSubjectsAsync();

        //Subjects = await ClassManager.GetSubjectsForClassAsync(int.Parse(SelectedClassId));
    }

    private async Task<List<Class>> GetClassesAsync()
    {
        /*return
        [
            new Class
            {
                Id = 1,
                Name = "Test"
            },
            new Class
            {
                Id = 2,
                Name = "Test"
            }
        ];*/
        var response = await HttpClient.GetAsync("api/class");
        //var response = HttpClient.GetFromJsonAsync<List<Class>>("api/class");

        // Check if the response indicates success
        if (!response.IsSuccessStatusCode)
        {
            // Handle non-success responses
            var errorContent = await response.Content.ReadFromJsonAsync<string>();
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Failure",
                Detail = errorContent,
                Duration = 4000,
                Style = "word-break:break-word"
            });
            //Message = $"Error: {response.StatusCode} - {errorContent}";
        }

        var classes = await response.Content.ReadFromJsonAsync<List<Class>>();
        return classes ?? [];
    }

    private async Task<List<SelectSubjectDto>> GetSubjectsAsync()
    {
        if (Input.AssignedClassId != 1) return [];

        return
        [
            new SelectSubjectDto
            {
                Id = 1,
                IsSelected = false,
                Name = "Subject1"
            },
            new SelectSubjectDto
            {
                Id = 10,
                IsSelected = false,
                Name = "Subject10"
            },
            new SelectSubjectDto
            {
                Id = 30,
                IsSelected = false,
                Name = "Subject30"
            }
        ];
    }

    private void AttachEventHandlers()
    {
        Input.RoleModified = async () =>
        {
            await RoleChanged();
            await ClassChanged();
        };
        Input.ClassModified = async () => await ClassChanged();
    }
}