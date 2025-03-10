using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using CerealApi.Data;
using CerealApi.Models;
using CerealApi.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CerealApi.Tests
{
    public class CerealControllerTests
    {
        private readonly CerealContext _context;
        private readonly CerealController _controller;

        public CerealControllerTests()
        {
            // Use an InMemory Database for testing
            var options = new DbContextOptionsBuilder<CerealContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class run
                .Options;

            _context = new CerealContext(options);

            // Seed some cereals
            _context.Cereals.AddRange(new List<Cereal>
            {
                new Cereal { Id = 1, Name = "Corn Flakes", Mfr = "K", Type="C", Calories = 100, Protein=2 },
                new Cereal { Id = 2, Name = "Frosted Flakes", Mfr = "K", Type="C", Calories = 110, Protein=2 },
                new Cereal { Id = 3, Name = "All-Bran", Mfr = "K", Type="C", Calories = 70,  Protein=4 },
                new Cereal { Id = 4, Name = "Choco Puffs", Mfr = "P", Type="C", Calories = 150, Protein=3, Carbo=10 },
            });
            _context.SaveChanges();

            _controller = new CerealController(_context);

            // By default, no user is authenticated (simulate endpoints that allow anonymous GET)
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        // -------------------------------------------------------------
        // 1. Testing GET /api/cereal with advanced filtering
        // -------------------------------------------------------------
        [Fact]
        public void GetAll_NoQueryParams_ReturnsAllCereals()
        {
            // Act
            var result = _controller.GetAll(new CerealQueryParams()) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var cereals = Assert.IsType<List<Cereal>>(result.Value);
            // We added 4 cereals in the constructor
            Assert.Equal(4, cereals.Count);
        }

        [Fact]
        public void GetAll_FilterByName_PartialMatch()
        {
            // Arrange
            var queryParams = new CerealQueryParams { Name = "flak" };

            // Act
            var result = _controller.GetAll(queryParams) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var cereals = Assert.IsType<List<Cereal>>(result.Value);
            // "Corn Flakes" and "Frosted Flakes" both contain "flak"
            Assert.Equal(2, cereals.Count);
        }

        [Fact]
        public void GetAll_FilterByExactMfrCaseInsensitive()
        {
            // Arrange
            var queryParams = new CerealQueryParams { Mfr = "k" };

            // Act
            var result = _controller.GetAll(queryParams) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var cereals = Assert.IsType<List<Cereal>>(result.Value);
            // 3 cereals with Mfr = "K": ID 1,2,3
            Assert.Equal(3, cereals.Count);
        }

        [Fact]
        public void GetAll_FilterByCaloriesRange()
        {
            // Example: >= 70 and <= 110
            var queryParams = new CerealQueryParams { CaloriesMin = 70, CaloriesMax = 110 };

            var result = _controller.GetAll(queryParams) as OkObjectResult;
            Assert.NotNull(result);

            var cereals = Assert.IsType<List<Cereal>>(result.Value);
            // ID=1 => 100 cals, ID=2 => 110 cals, ID=3 => 70 cals => all in range
            // ID=4 => 150 cals => out of range
            Assert.Equal(3, cereals.Count);
        }

        [Fact]
        public void GetAll_FilterByProteinLessThan()
        {
            // We want cereals with Protein <= 2
            var queryParams = new CerealQueryParams
            {
                ProteinMax = 2
            };

            var result = _controller.GetAll(queryParams) as OkObjectResult;
            Assert.NotNull(result);

            var cereals = Assert.IsType<List<Cereal>>(result.Value);
            // ID=1 => 2 protein, ID=2 => 2 protein, ID=3 => 4 protein, ID=4 => 3 protein
            // So only ID=1 and 2 remain
            Assert.Equal(2, cereals.Count);
        }

        // -------------------------------------------------------------
        // 2. Testing GET /api/cereal/{id}
        // -------------------------------------------------------------
        [Fact]
        public void GetById_Existing_ReturnsOk()
        {
            var result = _controller.GetById(1) as OkObjectResult;
            Assert.NotNull(result);
            var cereal = Assert.IsType<Cereal>(result.Value);
            Assert.Equal("Corn Flakes", cereal.Name);
        }

        [Fact]
        public void GetById_NonExisting_ReturnsNotFound()
        {
            var result = _controller.GetById(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // -------------------------------------------------------------
        // 3. Testing POST (create/update) that requires [Authorize]
        // -------------------------------------------------------------
        [Fact]
        public void Create_NoAuth_ReturnsUnauthorized()
        {
            // Attempt to create a cereal without being authorized
            // Because [Authorize] is on the endpoint
            var newCereal = new Cereal { Name = "New Unauthorized", Calories = 99 };

            var result = _controller.Create(newCereal);

            // If there's no user principal, the pipeline would normally return 401
            // However, in a pure unit test, [Authorize] is not triggered unless we mock the pipeline.
            // We'll check if we can forcibly interpret the result as Unauthorized or not.

            // By default, if we call the method directly, it won't check the attribute.
            // In real integration tests, you'd get 401.
            // We'll simulate returning 401 manually to demonstrate the approach:
            // We'll do that by verifying there's no user identity set.

            // Typical approach in a pure unit test:
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Create_NoId_CreatesNewCereal()
        {
            // Setup a user so we pass [Authorize]
            AuthorizeController(_controller);

            var newCereal = new Cereal { Name = "Honey Oats", Calories = 120, Mfr = "H" };

            var result = _controller.Create(newCereal) as CreatedAtActionResult;
            Assert.NotNull(result);
            var createdCereal = Assert.IsType<Cereal>(result.Value);
            Assert.True(createdCereal.Id != 0);
            // Check DB
            Assert.Equal(5, _context.Cereals.Count()); // We had 4, now 1 more
        }

        [Fact]
        public void Create_ExistingId_UpdatesExistingCereal()
        {
            AuthorizeController(_controller);

            // Let's update cereal ID=1 by passing that ID in POST
            var updateData = new Cereal { Id = 1, Name = "Corn Flakes Updated", Calories = 101 };

            var result = _controller.Create(updateData) as OkObjectResult;
            Assert.NotNull(result);

            var updated = Assert.IsType<Cereal>(result.Value);
            Assert.Equal(1, updated.Id);
            Assert.Equal("Corn Flakes Updated", updated.Name);
            Assert.Equal(101, updated.Calories);

            // Confirm changes in DB
            var dbCereal = _context.Cereals.Find(1);
            Assert.Equal("Corn Flakes Updated", dbCereal.Name);
        }

        [Fact]
        public void Create_NonExistingId_ReturnsError()
        {
            AuthorizeController(_controller);

            // ID=999 does not exist
            var badCereal = new Cereal { Id = 999, Name = "Should Fail" };

            var result = _controller.Create(badCereal);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("does not exist", badRequest.Value.ToString());
        }

        // -------------------------------------------------------------
        // 4. Testing PUT /api/cereal/{id} => update
        // -------------------------------------------------------------
        [Fact]
        public void Update_RequiresAuth_ReturnsUnauthorizedIfNoUser()
        {
            var updatedData = new Cereal { Id = 2, Name = "Updated Flakes" };

            // No user => we simulate pipeline => expect 401
            var result = _controller.Update(2, updatedData);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Update_ExistingId_UpdatesRecord()
        {
            AuthorizeController(_controller);

            var updatedData = new Cereal
            {
                Id = 2,
                Name = "Frosted Flakes X",
                Calories = 200,
                Mfr = "K"
            };

            var result = _controller.Update(2, updatedData) as OkObjectResult;
            Assert.NotNull(result);

            var updated = Assert.IsType<Cereal>(result.Value);
            Assert.Equal("Frosted Flakes X", updated.Name);
            Assert.Equal(200, updated.Calories);

            var fromDb = _context.Cereals.Find(2);
            Assert.Equal("Frosted Flakes X", fromDb.Name);
        }

        [Fact]
        public void Update_BadId_ReturnsBadRequest()
        {
            AuthorizeController(_controller);

            // ID mismatch => route param=2, but updatedCereal.Id=3
            var updatedData = new Cereal
            {
                Id = 3,
                Name = "Mismatch"
            };

            var result = _controller.Update(2, updatedData);
            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("invalid or ID mismatch", badReq.Value.ToString());
        }

        [Fact]
        public void Update_NonExistingId_ReturnsNotFound()
        {
            AuthorizeController(_controller);

            // There's no cereal with ID=999
            var updatedData = new Cereal
            {
                Id = 999,
                Name = "Ghost Cereal"
            };

            var result = _controller.Update(999, updatedData);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // -------------------------------------------------------------
        // 5. Testing DELETE /api/cereal/{id}
        // -------------------------------------------------------------
        [Fact]
        public void Delete_RequiresAuth_ReturnsUnauthorizedIfNoUser()
        {
            var result = _controller.Delete(1);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Delete_ExistingId_RemovesRecord()
        {
            AuthorizeController(_controller);

            var result = _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);

            // Confirm removal
            var check = _context.Cereals.Find(1);
            Assert.Null(check); // It's gone
        }

        [Fact]
        public void Delete_NonExisting_ReturnsNotFound()
        {
            AuthorizeController(_controller);

            var result = _controller.Delete(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // -------------------------------------------------------------
        // 6. Testing GET /api/cereal/{id}/image
        // -------------------------------------------------------------
        [Fact]
        public void GetImage_ExistingButNoImagePath_ReturnsPlaceholderOr404()
        {
            // ID=3 => "All-Bran", no ImagePath set
            var result = _controller.GetImage(3);

            // Because we have no actual file, it likely returns 404 
            // (unless you set up a placeholder in your local dir).
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetImage_NonExisting_ReturnsNotFound()
        {
            var result = _controller.GetImage(999);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        // If you want a test for a real image path, you'd mock or set up a real file.
        // We skip that as it requires local filesystem setup or deeper integration tests.

        // -------------------------------------------------------------
        // Helper methods
        // -------------------------------------------------------------

        /// <summary>
        /// Mocks an authorized user by setting a simple user identity on the controller's HttpContext.
        /// This simulates having a valid login token for the [Authorize] attribute.
        /// </summary>
        private void AuthorizeController(CerealController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "TestAuth"));

            controller.ControllerContext.HttpContext.User = user;
        }
    }
}
