using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Managers;
using SchoolManagerWeb.Components.Account.Pages;
using System.Net;

namespace SchoolManagerWeb.Endpoints;

public static class UserEndpoints
{
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        app.MapGet("hi", () => Results.Json("hello")).RequireAuthorization("RequireAdminRole");
        app.MapPost("user", async (UserRegistrationDto userDto, UserManager userManager, ILogger<Register> logger, IUserStore<User> userStore, IEmailSender<User> emailSender, ClassManager classManager) =>
        {
            var user = new User()
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName
            };
            IdentityError[] identityErrors;
            if (userDto.Role == "Student" && string.IsNullOrEmpty(userDto.SelectedClassId))
            {
                identityErrors = new[] { new IdentityError { Description = "Students must select a class." } };
                return Results.BadRequest(identityErrors);
            }

            await userManager.SetUserNameAsync(user, user.UserName);

            var emailStore = GetEmailStore(userManager, userStore);
            await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

            var result = await userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                identityErrors = result.Errors.ToArray();
                return Results.BadRequest(identityErrors);
            }

            logger.LogInformation($"New account has been created ({user.UserName})");

            Console.WriteLine("Input.Role:" + userDto.Role);
            await userManager.AddToRoleAsync(user, userDto.Role);
            //await UserManager.AssignRoleAsync(user, StringRoleConverter.GetRole("Admin"));

            /*if (userDto.Role == "Student" && !string.IsNullOrEmpty(userDto.SelectedClassId))
            {
                var selectedClass = Classes.First(cls => cls.Id == int.Parse(user.SelectedClassId));
                var selectedSubjects = Subjects.Where(s => s.IsSelected).Select(s => s.Id).ToList();
                // await ClassManager.AssignStudentToClassAsync(user, selectedClass, selectedSubjects);
            }

            /*var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await emailSender.SendConfirmationLinkAsync(user, user.Email, HtmlEncoder.Default.Encode(callbackUrl));*/



            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
                /*RedirectManager.RedirectTo(
                    "Account/RegisterConfirmation",
                    new() { ["email"] = user.Email, ["returnUrl"] = ReturnUrl });*/
                return Results.Ok("User created successfully. You need to confirm your email address.");
            }

            //RedirectManager.RedirectTo(ReturnUrl); */
            return Results.Ok("User created successfully");
        })
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .RequireAuthorization("RequireAdminRole");
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

}
