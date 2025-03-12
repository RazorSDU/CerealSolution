using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Force the content root to the main project's folder:
        builder.UseSolutionRelativeContentRoot("CerealApi");
        builder.UseEnvironment("Test");

        // Ensure Kestrel listens on HTTPS
        builder.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5001, listenOptions =>
            {
                listenOptions.UseHttps(); // Force HTTPS support in tests
            });
        });

        base.ConfigureWebHost(builder);
    }
}
