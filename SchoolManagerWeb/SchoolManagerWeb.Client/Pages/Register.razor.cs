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
    private bool ClassesAreLoading { get; set; }
    private bool SubjectsAreLoading { get; set; }
    private bool RegistrationIsPending { get; set; }
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    const int TimeoutMilliseconds = 15000;

    private List<int> SelectedSubjects => Subjects
        .Where(x => x.IsSelected)
        .Select(s => s.Id)
        .ToList();

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

            var cts = new CancellationTokenSource(TimeoutMilliseconds);
            RegistrationIsPending = true;
            var response = await HttpClient.PostAsJsonAsync("user", Input, cts.Token);

            if (response.IsSuccessStatusCode)
            {
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
                var errorContent = await response.Content.ReadFromJsonAsync<IdentityError[]>(cts.Token) ?? [];
                ShowFailureNotification(
                    $"Error during user adding!\n{string.Join("\n", errorContent.Select(x => x.Description))}");
            }
        }
        catch (OperationCanceledException)
        {
            ShowFailureNotification("Network timeout occurred");
        }
        catch (HttpRequestException)
        {
            ShowFailureNotification($"Network error occurred");
        }
        catch (Exception ex)
        {
            ShowFailureNotification($"Exception: {ex.Message}");
        }
        finally
        {
            RegistrationIsPending = false;
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
            return;
        }

        SubjectsAreLoading = true;
        Subjects = await GetSubjectsAsync();
        SubjectsAreLoading = false;
        StateHasChanged();
    }

    private async Task<List<Class>> GetClassesAsync()
    {
        using var cts = new CancellationTokenSource(TimeoutMilliseconds);

        try
        {
            var response = await HttpClient.GetAsync("api/class", cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadFromJsonAsync<string>(cancellationToken: cts.Token) ??
                                   "Failed to get classes";
                ShowFailureNotification(errorContent);
                return [];
            }

            var classes = await response.Content.ReadFromJsonAsync<List<Class>>(cancellationToken: cts.Token);
            return classes ?? [];
        }
        catch (OperationCanceledException)
        {
            ShowFailureNotification("Network timeout occurred");
            return [];
        }
        catch (HttpRequestException)
        {
            ShowFailureNotification($"Network error occurred");
            return [];
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

    private async Task<List<SelectSubjectDto>> GetSubjectsAsync()
    {
        using var cts = new CancellationTokenSource(TimeoutMilliseconds);

        try
        {
            var response = await HttpClient.GetAsync($"api/class/{Input.AssignedClassId}/subjects", cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var subjects =
                    await response.Content.ReadFromJsonAsync<List<GetSubjectDto>>(cancellationToken: cts.Token) ?? [];
                return subjects.Select(x => new SelectSubjectDto(x.Id, x.Name)).ToList();
            }

            // Handle non-success responses
            var errorContent = await response.Content.ReadFromJsonAsync<string>(cancellationToken: cts.Token)
                               ?? "Failed to get subjects";
            ShowFailureNotification(errorContent);
        }
        catch (OperationCanceledException)
        {
            ShowFailureNotification("Network timeout occurred");
        }
        catch (HttpRequestException)
        {
            ShowFailureNotification("Network error occurred");
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
}