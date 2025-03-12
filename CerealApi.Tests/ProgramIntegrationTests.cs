using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CerealApi.Tests
{
    // WebApplicationFactory<Program> spins up your entire ASP.NET Core app in memory
    public class ProgramIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ProgramIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllCereals_ReturnsOk()
        {
            // Arrange
            // Create a test client that can talk to the in-memory server
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost") // Force HTTPS for all test requests
            });


            // Act
            // Call your /api/cereal endpoint
            var response = await client.GetAsync("/api/cereal");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Optionally parse JSON, check content, etc.
            // string content = await response.Content.ReadAsStringAsync();
            // Assert.Contains("Froot Loops", content);
        }

        [Fact]
        public async Task Swagger_Endpoint_Should_Exist_In_Development()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/index.html");

            // If you're in Development by default, swagger should return 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_Endpoint_Should_Not_Exist_In_Production()
        {
            // Override environment to Production
            var prodFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ENVIRONMENT", "Production");
            });

            // Force HTTPS so the "block HTTP" middleware doesn't trigger a 403
            var client = prodFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var response = await client.GetAsync("/swagger/index.html");

            // Expect 404 since Swagger isn't enabled in Production
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task API_Requires_HTTPS_For_Cereal_Endpoint()
        {
            // Force HTTP (not HTTPS)
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost") // Ensure it's an HTTP request
            });

            // Attempt to make a CRUD call over HTTP
            var response = await client.GetAsync("/api/cereal");

            // Expect failure since HTTPS is required
            Assert.True(
                response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected Forbidden (403) or Bad Request (400), but got {response.StatusCode}"
            );
        }

        [Fact]
        public async Task RateLimit_Should_Return_429()
        {
            // Create a fresh factory with rate limiting explicitly enabled for this test
            using var rateLimitFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("RateLimitTest"); // Override to enable rate limiting
                });

            var client = rateLimitFactory.CreateClient();
            HttpStatusCode? lastStatus = null;

            // Make enough requests to exceed the rate limit
            for (int i = 0; i < 10; i++)  // Adjust based on PermitLimit in Program.cs
            {
                var resp = await client.GetAsync("/api/cereal");
                if (!resp.IsSuccessStatusCode)
                {
                    lastStatus = resp.StatusCode;
                    break;
                }
            }

            Assert.Equal(HttpStatusCode.TooManyRequests, lastStatus);
        }

    }
}
