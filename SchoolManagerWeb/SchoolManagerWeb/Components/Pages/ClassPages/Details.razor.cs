﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;

namespace SchoolManagerWeb.Components.Pages.ClassPages;

public partial class Details
{
    [Inject] public required DialogService DialogService { get; set; }
    private Class? currentClass;
    private List<User> Students = [];
    private List<Subject> Subjects = [];
    [SupplyParameterFromQuery] private int Id { get; set; }

    private EventCallback<int> OnSelectedTabChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        currentClass = await context.Classes.FirstOrDefaultAsync(m => m.Id == Id);

        if (currentClass is null)
        {
            NavigationManager.NavigateTo("notfound");
            return;
        }

        var result = await ClassManager.GetClassStudentsAsync(currentClass);

        Students = result;
        OnSelectedTabChanged = EventCallback.Factory.Create<int>(this, OnTabIndexChangedAsync);
    }

    private async Task OnTabIndexChangedAsync(int tabIndex)
    {
        // Handle tab change logic asynchronously
        switch (tabIndex)
        {
            case 0:
                await FetchStudentsAsync();
                break;
            case 1:
                await FetchSubjectsAsync();
                break;
        }

        //_selectedTabIndex = tabIndex;
    }

    private async Task FetchStudentsAsync()
    {
        if (currentClass is null)
        {
            return;
        }

        Students = await ClassManager.GetClassStudentsAsync(currentClass);
        StateHasChanged();
    }

    private async Task FetchSubjectsAsync()
    {
        if (currentClass is null)
        {
            return;
        }

        Subjects = await ClassManager.GetClassSubjectsAsync(currentClass);
        StateHasChanged();
    }

    private void OpenAddSubjectDialog()
    {
        if (currentClass is null)
        {
            return;
        }

        DialogService.OnClose += async _ => await FetchSubjectsAsync();
        DialogService.Open<AddSubject>("Add subject", new Dictionary<string, object>
        {
            { "SelectedClass", currentClass }
        });
    }
}