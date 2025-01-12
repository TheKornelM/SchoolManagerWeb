using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Identity.Client;
using Mono.TextTemplating;
using Radzen;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Extensions;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Utils;
using SchoolManagerModel.Validators;

namespace SchoolManagerWeb.Components.Pages;

public partial class Register
{
    [Inject] public required NotificationService NotificationService { get; set; }
    [Inject] public required UserManager UserManager { get; set; }
    [Inject] public required ClassManager ClassManager { get; set; }
    [Inject] public required SubjectManager SubjectManager { get; set; }
    [Inject] public required IUserStore<User> UserStore { get; set; }
    [Inject] public required ILogger<Register> Logger { get; set; }

    [SupplyParameterFromForm] private UserRegistrationDto Input { get; set; } = new();
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    private const int TimeoutMilliseconds = 15000;

    private List<Class> Classes { get; set; } = [];
    private List<Subject> Subjects { get; set; } = [];
    private IEnumerable<int> SelectedSubjectIds { get; set; } = [];
    private string? Message { get; set; }
    private bool ClassesAreLoading { get; set; }
    private bool SubjectsAreLoading { get; set; }
    private bool RegistrationIsPending { get; set; }
    private IEnumerable<IdentityError>? identityErrors;

    private List<int> SelectedSubjects => SelectedSubjectIds.ToList();

    protected override async Task OnInitializedAsync()
    {
        AttachEventHandlers();
    }

    private async Task RegisterUser(EditContext editContext)
    {
        try
        {
            if (Input.Role == "Student")
            {
                Input.AssignedSubjects = SelectedSubjects;
            }

            RegistrationIsPending = true;

            if (Input is { Role: "Student", AssignedClassId: null })
            {
                identityErrors = new[] { new IdentityError { Description = "Students must select a class." } };
                return;
            }

            var user = Input.ToUser();

            var userValidator =
                new ValidNotExistsUserValidator(UserManager, UIResourceFactory.GetNewResource());
            var validationResult = await userValidator.ValidateAsync(user);

            if (!validationResult.IsValid)
            {
                identityErrors = validationResult.Errors.Select(error => new IdentityError
                {
                    Description = error.ErrorMessage
                }).ToList();

                ShowIdentityErrorsNotification();
                return;
            }

            await UserManager.SetUserNameAsync(user, Input.Username);
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;

            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await UserManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                identityErrors = result.Errors;
                ShowIdentityErrorsNotification();
                return;
            }

            await UserManager.AddToRoleAsync(user, Input.Role);

            var role = StringRoleConverter.GetRole(Input.Role);
            switch (role)
            {
                case Role.Student:
                    var selectedClass = Classes.First(cls => cls.Id == Input.AssignedClassId);
                    var selectedSubjects = Subjects
                        .Where(subject => Input.AssignedSubjects.Contains(subject.Id))
                        .ToList();

                    var student = new Student
                    {
                        User = user,
                        Class = selectedClass!
                    };
                    await UserManager.AddStudentAsync(student);
                    await SubjectManager.AssignSubjectsToStudentAsync(student, selectedSubjects);
                    break;
                case Role.Teacher:
                    var teacher = new Teacher
                    {
                        User = user,
                    };
                    await UserManager.AddTeacherAsync(teacher);
                    break;
                case Role.Administrator:
                    var admin = new Admin
                    {
                        User = user,
                    };
                    await UserManager.AddAdminAsync(admin);
                    break;
            }

            Logger.LogInformation($"New user has been created (username: {user.UserName}, email: {user.Email})");

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = "You added user successfully",
                Duration = 4000,
                Style = "word-break:break-word"
            });

            Input = new UserRegistrationDto();
            SelectedSubjectIds = [];
            AttachEventHandlers();
        }
        catch (Exception ex)
        {
            ShowFailureNotification($"Error during user adding!\\n: {ex.Message}");
        }
        finally
        {
            RegistrationIsPending = false;
        }
    }

    private async Task RoleChanged()
    {
        if (string.IsNullOrEmpty(Input.Role)) return;

        Input.AssignedClassId = null;
        Subjects.Clear();

        if (Input.Role == "Student")
        {
            SelectedSubjectIds = [];
            ClassesAreLoading = true;
            Classes = await GetClassesAsync();
            ClassesAreLoading = false;
            StateHasChanged();
        }
        else
        {
            Classes.Clear();
        }
    }

    private async Task ClassChanged()
    {
        if (Input.AssignedClassId == null)
        {
            Subjects.Clear();
            SelectedSubjectIds = [];
            return;
        }

        SubjectsAreLoading = true;
        Subjects = await GetSubjectsAsync();
        SubjectsAreLoading = false;
        StateHasChanged();
    }

    private async Task<List<Class>> GetClassesAsync()
    {
        try
        {
            var classes = await ClassManager.GetClassesAsync();

            return classes?.OrderBy(x => x.Year)
                .ThenBy(x => x.SchoolClass)
                .ThenBy(x => x.Id).ToList() ?? [];
        }
        catch (Exception)
        {
            ShowFailureNotification($"Unexpected error occurred");
            return [];
        }
        finally
        {
            ClassesAreLoading = false;
            StateHasChanged();
        }
    }

    private async Task<List<Subject>> GetSubjectsAsync()
    {
        try
        {
            var @class = Classes.First(x => x.Id == Input.AssignedClassId);
            var subjects = await ClassManager.GetClassSubjectsAsync(@class);
            return subjects;
        }
        catch (Exception)
        {
            ShowFailureNotification("Unexpected error occurred");
        }
        finally
        {
            ClassesAreLoading = false;
            StateHasChanged();
        }

        return [];
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

    private void ShowFailureNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000,
            Style = "word-break:break-word"
        });
    }

    public void ShowIdentityErrorsNotification()
    {
        var errors = identityErrors?.Select(x => x.Description);

        if (errors != null)
        {
            ShowFailureNotification(string.Join('\n', errors));
        }
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!UserManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<User>)UserStore;
    }
}