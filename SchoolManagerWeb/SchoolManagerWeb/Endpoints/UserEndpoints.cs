using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using SchoolManagerModel.DTOs;
using SchoolManagerModel.Entities;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Extensions;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Utils;
using SchoolManagerModel.Validators;
using SchoolManagerWeb.Components.Account.Pages;

namespace SchoolManagerWeb.Endpoints;

public static class UserEndpoints
{
    public static WebApplication AddUserEndpoints(this WebApplication app)
    {
        app.MapPost("user",
                async Task<Results<Ok<string>, BadRequest<List<IdentityError>>, InternalServerError>> (
                    UserRegistrationDto userDto,
                    UserManager userManager,
                    ILogger<Register> logger, IUserStore<User> userStore, IEmailSender<User> emailSender,
                    ClassManager classManager, SubjectManager subjectManager) =>
                {
                    List<IdentityError> identityErrors = [];

                    // User validator
                    var user = userDto.ToUser();

                    var userValidator =
                        new ValidNotExistsUserValidator(userManager, UIResourceFactory.GetNewResource());
                    var validationResult = await userValidator.ValidateAsync(user);

                    if (!validationResult.IsValid)
                    {
                        identityErrors = validationResult.Errors.Select(error => new IdentityError
                        {
                            Description = $"{error.PropertyName}: {error.ErrorMessage}"
                        }).ToList();

                        return TypedResults.BadRequest(identityErrors);
                    }

                    // Role validator
                    if (StringRoleConverter.GetRole(userDto.Role) == null)
                    {
                        identityErrors.Add(new IdentityError
                        {
                            Description = $"Given role ({userDto.Role}) is not valid role."
                        });

                        return TypedResults.BadRequest(identityErrors);
                    }

                    Class? @class = null;

                    // Student validator
                    var studentSubjects = new List<Subject>();
                    var role = StringRoleConverter.GetRole(userDto.Role);
                    if (role == Role.Student)
                    {
                        // A student must have an assigned class
                        if (userDto.AssignedClassId == null)
                        {
                            identityErrors.Add(
                                new IdentityError { Description = "Student must have a selected class." });
                            return TypedResults.BadRequest(identityErrors);
                        }

                        // Check that class is valid
                        var classes = await classManager.GetClassesAsync();
                        @class = classes.FirstOrDefault(x => x.Id == userDto.AssignedClassId);
                        if (@class == null)
                        {
                            identityErrors.Add(new IdentityError { Description = "Given class ID not found." });
                            return TypedResults.BadRequest(identityErrors);
                        }

                        // Check that subject ids are valid and assigned to the given class

                        userDto.AssignedSubjects ??= [];

                        if (userDto.AssignedSubjects.Distinct().Count() != userDto.AssignedSubjects.Count)
                        {
                            identityErrors.Add(new IdentityError
                            { Description = "Given subject IDs must be unique." });
                        }

                        var classSubjects = await classManager.GetClassSubjectsAsync(@class);
                        studentSubjects = classSubjects
                            .IntersectBy(userDto.AssignedSubjects, x => x.Id).ToList();

                        if (studentSubjects.Count != userDto.AssignedSubjects.Count)
                        {
                            identityErrors.Add(
                                new IdentityError { Description = "Given subject IDs must exist." });
                            return TypedResults.BadRequest(identityErrors);
                        }
                    }

                    var emailStore = GetEmailStore(userManager, userStore);
                    await userManager.SetUserNameAsync(user, user.UserName);
                    await emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);
                    var result = await userManager.CreateAsync(user, userDto.Password);

                    if (!result.Succeeded)
                    {
                        identityErrors = result.Errors.ToList();
                        return TypedResults.BadRequest(identityErrors);
                    }

                    logger.LogInformation($"New account has been created ({user.UserName})");

                    await userManager.AddToRoleAsync(user, userDto.Role);

                    switch (role)
                    {
                        case Role.Student:
                            var student = new Student
                            {
                                User = user,
                                Class = @class!
                            };
                            await userManager.AddStudentAsync(student);
                            await subjectManager.AssignSubjectsToStudentAsync(student, studentSubjects);
                            break;
                        case Role.Teacher:
                            var teacher = new Teacher
                            {
                                User = user,
                            };
                            await userManager.AddTeacherAsync(teacher);
                            break;
                        case Role.Administrator:
                            var admin = new Admin
                            {
                                User = user,
                            };
                            await userManager.AddAdminAsync(admin);
                            break;
                        default:
                            return TypedResults.InternalServerError();
                    }

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

                    return TypedResults.Ok(userManager.Options.SignIn.RequireConfirmedAccount
                        ? "User created successfully. User needs to confirm the email address."
                        : "User created successfully");
                })
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