using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;

namespace SchoolManagerWeb.Client.Pages;

public partial class Register
{
    private IEnumerable<IdentityError>? identityErrors;

    [Inject] public required HttpClient HttpClient { get; set; }

    [SupplyParameterFromForm] private UserRegistrationDto Input { get; set; } = new();

    private List<Class> Classes { get; set; } = [];
    private List<SelectSubjectDto> Subjects { get; set; } = [];
    private string? Message { get; set; }

    private List<int> SelectedSubjects => Subjects
        .Where(x => x.IsSelected)
        .Select(s => s.Id)
        .ToList();

    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //Classes = await ClassManager.GetClassesAsync();
        AttachEventHandler();
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
            // Make the API call
            var response = await HttpClient.PostAsJsonAsync("user", Input);

            // Check if the response indicates success
            if (response.IsSuccessStatusCode)
            {
                // Read and display the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Message = $"Success: {responseContent}";
                Input = new UserRegistrationDto();
                AttachEventHandler();
            }
            else
            {
                // Handle non-success responses
                var errorContent = await response.Content.ReadAsStringAsync();
                Message = $"Error: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the API call
            Message = $"Exception: {ex.Message}";
        }

        Console.WriteLine(Message); // Log the message for debugging
    }

    private async void RoleChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(Input.Role)) return;

        if (Input.Role == "Student")
        {
            Input.AssignedClassId = null;
            Subjects.Clear();
            Classes = await GetClassesAsync();
        }
        else
        {
            Input.AssignedClassId = null;
            Subjects.Clear();
            Classes.Clear();
        }
    }

    private async void ClassChanged(object? sender, EventArgs e)
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
        return
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
        ];
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

    private void AttachEventHandler()
    {
        Input.RoleModified += RoleChanged;
        Input.RoleModified += ClassChanged;
        Input.ClassModified += ClassChanged;
    }
}