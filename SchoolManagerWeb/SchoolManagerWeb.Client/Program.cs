using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace SchoolManagerWeb.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthenticationStateDeserialization();

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost/")
            });

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>());
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<NotificationService>();


            await builder.Build().RunAsync();
        }
    }
}