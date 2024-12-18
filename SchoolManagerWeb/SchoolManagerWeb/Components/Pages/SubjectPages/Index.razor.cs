using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Extensions;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Utils;
using SchoolManagerWeb.Utils;

namespace SchoolManagerWeb.Components.Pages.SubjectPages;

public partial class Index
{
    [Inject] public required UserManager UserManager { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required TeacherManager TeacherManager { get; set; }
    [Inject] public required SubjectManager SubjectManager { get; set; }
    [Inject] public required Notifier Notifier { get; set; }
    [Inject] public required DialogService DialogService { get; set; }

    private User? _user;
    private Subject? _selectedSubject;
    private Student? _selectedStudent;
    private List<Subject> Subjects { get; set; } = [];

    private Subject? SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            _selectedSubject = value;
            SubjectChanged.InvokeAsync();
        }
    }

    private List<Student> SelectedSubjectStudents { get; set; } = [];

    private Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;
            StudentChanged.InvokeAsync();
        }
    }

    private EventCallback SubjectChanged { get; set; }
    private EventCallback StudentChanged { get; set; }

    private List<Mark> SelectedStudentMarks { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var username = await AuthenticationStateProvider.GetUsername();

        if (string.IsNullOrWhiteSpace(username))
        {
            NavigationManager.NavigateTo($"notfound");
            return;
        }

        _user = await UserManager.GetUserByUsernameAsync(username);
        var subjects = await GetSubjects();

        if (subjects == null)
        {
            Notifier.ShowError("There was an error loading the subject list");
            NavigationManager.NavigateTo($"notfound");
            return;
        }

        Subjects = subjects;
        SubjectChanged = EventCallback.Factory.Create(this, GetStudentsAsync);
        StudentChanged = EventCallback.Factory.Create(this, GetMarksAsync);
    }

    private async Task<List<Subject>?> GetSubjects()
    {
        if (_user == null)
        {
            return null;
        }

        var roles = await UserManager.GetRolesAsync(_user);
        var role = StringRoleConverter.GetRole(roles.FirstOrDefault() ?? string.Empty);

        switch (role)
        {
            case Role.Teacher:
                var teacher = await UserManager.GetTeacherByUserAsync(_user);

                if (teacher == null)
                {
                    NavigationManager.NavigateTo($"notfound");
                    return [];
                }

                return await TeacherManager.GetCurrentTaughtSubjectsAsync(teacher);

            default:
                return null;
        }
    }

    private async Task GetStudentsAsync()
    {
        SelectedStudent = null;
        if (SelectedSubject == null)
        {
            return;
        }

        SelectedSubjectStudents = await SubjectManager.GetSubjectStudentsAsync(SelectedSubject);
    }

    private async Task GetMarksAsync()
    {
        SelectedStudentMarks = [];

        if (_selectedStudent == null || _selectedSubject == null)
        {
            return;
        }

        SelectedStudentMarks = await SubjectManager.GetStudentSubjectMarksAsync(_selectedStudent, _selectedSubject);
    }

    private void OpenAddMarkDialog()
    {
        if (SelectedSubject == null || SelectedStudent == null)
        {
            return;
        }

        DialogService.OnClose += async _ =>
        {
            await GetMarksAsync();
            StateHasChanged();
        };

        DialogService.Open<AddSubjectMark>("Add mark", new Dictionary<string, object>
        {
            { "Subject", SelectedSubject },
            { "Student", SelectedStudent },
        });
    }
}