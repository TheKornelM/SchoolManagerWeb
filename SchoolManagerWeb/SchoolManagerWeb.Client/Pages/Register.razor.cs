using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;
using System.Net.Http.Json;

namespace SchoolManagerWeb.Client.Pages;

public partial class Register
{
    [Inject]
    public required HttpClient HttpClient { get; set; }

    [SupplyParameterFromForm]
    private UserRegistrationDto Input { get; set; } = new();
    private List<Class> Classes { get; set; } = new();
    private List<SubjectSelection> Subjects { get; set; } = new();
    private string? SelectedClassId { get; set; } = null;

    private string? Message { get; set; }
    private IEnumerable<IdentityError>? identityErrors;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //Classes = await ClassManager.GetClassesAsync();
        Classes = new List<Class>()
        {
            new Class()
            {
                Id = 1,
                Name = "Test"
            }
        };

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

    private class SubjectSelection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }

    private async Task RoleChanged(ChangeEventArgs e)
    {
        Console.WriteLine("RoleChanged event");
        if (!string.IsNullOrEmpty(Input.Role))
        {
            Console.WriteLine("ok");

            if (Input.Role == "Student")
            {
                SelectedClassId = null;
                Subjects.Clear();
            }
            else
            {
                SelectedClassId = null;
                Subjects.Clear();
                Classes.Clear();
            }
        }
    }

    private async Task ClassChanged()
    {
        if (!string.IsNullOrEmpty(SelectedClassId))
        {
            //Subjects = await ClassManager.GetSubjectsForClassAsync(int.Parse(SelectedClassId));
        }
        else
        {
            Subjects.Clear();
        }
    }

    private async Task OnRoleChanged(ChangeEventArgs e)
    {
        // await RoleChanged();
    }

    private async Task OnClassChanged(ChangeEventArgs e)
    {
        await ClassChanged();
    }

}
