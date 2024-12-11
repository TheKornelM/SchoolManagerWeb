using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Managers;
using SchoolManagerWeb.Components.Account.Pages;

namespace SchoolManagerEndpoints;

public static class UserEndpoints
{
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/hi", () => " hello");
        app.MapPost("user", async (UserRegistrationDto userDto, UserManager userManager, ILogger<Register> logger, IUserStore<User> userStore, IEmailSender<User> emailSender, ClassManager classManager) =>
        {
            var user = new User()
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            };
            IdentityError[] identityErrors;
            if (userDto.Role == "Student" && string.IsNullOrEmpty(userDto.SelectedClassId))
            {
                identityErrors = new[] { new IdentityError { Description = "Students must select a class." } };
                return TypedResults.BadRequest(identityErrors);
            }

            await userManager.SetUserNameAsync(user, user.UserName);
            user.FirstName = user.FirstName;
            user.LastName = user.LastName;

            var emailStore = GetEmailStore(userManager, userStore);
            await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                identityErrors = result.Errors;
                return TypedResults.BadRequest(identityErrors);
            }

            logger.LogInformation("User created a new account with password.");

            Console.WriteLine("Input.Role:" + userDto.Role);
            await userManager.AddToRoleAsync(user, userDto.Role);
            //await UserManager.AssignRoleAsync(user, StringRoleConverter.GetRole("Admin"));

            /*if (userDto.Role == "Student" && !string.IsNullOrEmpty(userDto.SelectedClassId))
            {
                var selectedClass = Classes.First(cls => cls.Id == int.Parse(user.SelectedClassId));
                var selectedSubjects = Subjects.Where(s => s.IsSelected).Select(s => s.Id).ToList();
                // await ClassManager.AssignStudentToClassAsync(user, selectedClass, selectedSubjects);
            }*/

            /*var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await emailSender.SendConfirmationLinkAsync(user, user.Email, HtmlEncoder.Default.Encode(callbackUrl));



            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
                RedirectManager.RedirectTo(
                    "Account/RegisterConfirmation",
                    new() { ["email"] = user.Email, ["returnUrl"] = ReturnUrl });
            }

            RedirectManager.RedirectTo(ReturnUrl);*/
            return TypedResults.Ok("User created successfully");
        })
            .Produces(TypedResults.Ok)
            .Produces(TypedResults.BadRequest);
        return app;
    }

    private static IUserEmailStore<User> GetEmailStore(UserManager UserManager, IUserStore<User> UserStore)
    {
        if (!UserManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)UserStore;
    }

    /*private static async Task RegisterUser(UserRegistrationDto Input, UserManager UserManager)
    {
        IdentityError[] identityErrors;
        if (Input.Role == "Student" && string.IsNullOrEmpty(Input.SelectedClassId))
        {
            identityErrors = new[] { new IdentityError { Description = "Students must select a class." } };
            return;
        }

        var user = CreateUser();
        await UserManager.SetUserNameAsync(user, Input.Username);
        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;

        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await UserManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            identityErrors = result.Errors;
            return;
        }

        Logger.LogInformation("User created a new account with password.");

        Console.WriteLine("Input.Role:" + Input.Role);
        await UserManager.AddToRoleAsync(user, Input.Role);
        //await UserManager.AssignRoleAsync(user, StringRoleConverter.GetRole("Admin"));

        if (Input.Role == "Student" && !string.IsNullOrEmpty(SelectedClassId))
        {
            var selectedClass = Classes.First(cls => cls.Id == int.Parse(SelectedClassId));
            var selectedSubjects = Subjects.Where(s => s.IsSelected).Select(s => s.Id).ToList();
            // await ClassManager.AssignStudentToClassAsync(user, selectedClass, selectedSubjects);
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

        await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));



        if (UserManager.Options.SignIn.RequireConfirmedAccount)
        {
            RedirectManager.RedirectTo(
                "Account/RegisterConfirmation",
                new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
        }

        RedirectManager.RedirectTo(ReturnUrl);
    }

    private static User CreateUser()
    {
        try
        {
            return Activator.CreateInstance<User>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. Ensure it's not abstract and has a parameterless constructor.");
        }
    }

    private static IUserEmailStore<User> GetEmailStore(IUserStore<User> UserStore, UserManager UserManager)
    {
        if (!UserManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)UserStore;
    }*/

}
