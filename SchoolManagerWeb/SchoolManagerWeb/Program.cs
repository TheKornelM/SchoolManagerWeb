using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SchoolManagerModel.Entities.UserModel;
using SchoolManagerModel.Managers;
using SchoolManagerModel.Persistence;
using SchoolManagerWeb.Components;
using SchoolManagerWeb.Components.Account;
using SchoolManagerWeb.Endpoints;


namespace SchoolManagerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();

            builder.Configuration
                .AddEnvironmentVariables();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole",
                    policy => policy.RequireRole("Admin"));
            });

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                   throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSeeding((context, _) =>
                {
                    // Return if account exists with "1" ID (admin)
                    if (context.Set<User>().Any(x => x.Id == "1"))
                    {
                        return;
                    }

                    // Add admin user
                    var hasher = new PasswordHasher<User>();
                    var adminUser = new User()
                    {
                        Id = "1",
                        UserName = "admin",
                        Email = "admin@admin.admin",
                        NormalizedEmail = "ADMIN@ADMIN.ADMIN",
                        NormalizedUserName = "ADMIN",
                        FirstName = "admin",
                        LastName = "admin"
                    };
                    adminUser.PasswordHash = hasher.HashPassword(adminUser, "admin");
                    context.Set<User>().Add(adminUser);

                    // Add to admin table
                    var admin = new Admin()
                    {
                        Id = 1,
                        User = adminUser
                    };
                    context.Set<Admin>().Add(admin);

                    // Assign role
                    context.Set<IdentityUserRole<string>>().Add(new IdentityUserRole<string>()
                    {
                        RoleId = "1",
                        UserId = "1",
                    });

                    context.SaveChanges();
                });
            });

            builder.Services.AddQuickGridEntityFrameworkAdapter();

            // Add SchoolManager service and repository classes
            builder.Services.AddTransient<SchoolDbContextBase, SchoolDbContext>();
            builder.Services.AddScoped<IAsyncClassDataHandler, ClassDatabase>();
            builder.Services.AddScoped<IAsyncSubjectDataHandler, SubjectDatabase>();
            builder.Services.AddScoped<IAsyncUserDataHandler, UserDatabase>();
            builder.Services.AddScoped<IAsyncTeacherDataHandler, TeacherDatabase>();
            builder.Services.AddScoped<UserManager>();
            builder.Services.AddScoped<ClassManager>();
            builder.Services.AddScoped<SubjectManager>();
            builder.Services.AddScoped<TeacherManager>();
            builder.Services.AddDbContextFactory<SchoolDbContext>(options => { options.UseNpgsql(connectionString); },
                ServiceLifetime.Scoped);
            /*builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SchoolManagerWeb",
                    Version = "v1"
                });
            });*/

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("ServerAPI", client => client.BaseAddress = new Uri("https://localhost/"));
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>());

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<User>(options =>
                {
                    //options.SignIn.RequireConfirmedAccount = true;
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SchoolDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<NotificationService>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
                /*app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
                });*/
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseMigrationsEndPoint();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();
            app.AddUserEndpoints();
            app.MapClassEndpoints();

            app.UseAntiforgery();

            app.Run();
        }
    }
}