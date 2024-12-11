using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.Entities;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace SchoolManagerWeb.Client.Pages;

public partial class Register
{
    [Inject]
    public required HttpClient HttpClient { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();
    private List<Class> Classes { get; set; } = new();
    private List<SubjectSelection> Subjects { get; set; } = new();
    private string? SelectedClassId { get; set; } = string.Empty;

    private string? Message { get; set; }
    private IEnumerable<IdentityError>? identityErrors;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        //Classes = await ClassManager.GetClassesAsync();
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
        var response = await HttpClient.PostAsJsonAsync("user", Input);
        Message = response.ToString();
    }

    private class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = "";

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = "";

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "Teacher";
    }

    private class SubjectSelection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }

    private async Task RoleChanged()
    {
        if (!string.IsNullOrEmpty(Input.Role))
        {
            if (Input.Role == "Student")
            {
                SelectedClassId = null;
                Subjects.Clear();
                //Classes = await ClassManager.GetClassesAsync();
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
        await RoleChanged();
    }

    private async Task OnClassChanged(ChangeEventArgs e)
    {
        await ClassChanged();
    }

}
