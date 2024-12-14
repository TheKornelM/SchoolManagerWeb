using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Utils;
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

            List<IdentityError> identityErrors = [];

            // User validator

            // Role validator
            if (StringRoleConverter.GetRole(userDto.Role) == null)
            {
                identityErrors.Add(new IdentityError()
                {
                    Description = $"Given role ({userDto.Role}) is not valid role."
                });

                return Results.BadRequest(identityErrors);
            }

            // Student validator
            if (userDto.Role == "Student")
            {
                // A student must have an assigned class
                if (userDto.AssignedClassId == null)
                {
                    identityErrors.Add(new IdentityError { Description = "Students must select a class." });
                    return Results.BadRequest(identityErrors);
                }

                // Check that class is valid

                // Check that subject ids are valid and assigned to this class
            }

            var emailStore = GetEmailStore(userManager, userStore);
            var user = new User()
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName
            };

            await userManager.SetUserNameAsync(user, user.UserName);
            await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);
            var result = await userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                identityErrors = result.Errors.ToList();
                return Results.BadRequest(identityErrors);
            }

            logger.LogInformation($"New account has been created ({user.UserName})");

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
