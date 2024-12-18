using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Extensions;
using SchoolManagerModel.Managers;

namespace SchoolManagerWeb.Components;

public partial class ShowMarks : ComponentBase
{
    [Inject] public required SubjectManager SubjectManager { get; set; }
    [Inject] public required UserManager UserManager { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }

    private User? _user;
    private List<Mark> _marks = [];

    protected override async Task OnInitializedAsync()
    {
        var username = await AuthenticationStateProvider.GetUsername();

        if (string.IsNullOrWhiteSpace(username))
        {
            NavigationManager.NavigateTo($"notfound");
            return;
        }

        _user = await UserManager.GetUserByUsernameAsync(username);

        if (_user == null)
        {
            NavigationManager.NavigateTo($"notfound");
            return;
        }

        _marks = await GetMarksAsync();
    }

    private async Task<List<Mark>> GetMarksAsync()
    {
        var student = await GetStudentAsync();

        if (student is null)
        {
            return [];
        }

        return await SubjectManager.GetStudentMarksAsync(student);
    }

    private async Task<Student?> GetStudentAsync()
    {
        if (_user is null)
        {
            return null;
        }

        return await UserManager.GetStudentByUserAsync(_user);
    }
}