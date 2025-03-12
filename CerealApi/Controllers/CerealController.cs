using CerealApi.Data;
using CerealApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;

namespace CerealApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CerealController : ControllerBase
    {
        private readonly CerealContext _context;
        private readonly ILogger<CerealController> _logger;

        public CerealController(CerealContext context, ILogger<CerealController> logger)
        {
            _context = context; // Now Scoped, reusing the same instance per request
            _logger = logger;
        }

        /// <summary>
        /// GET all cereals, or filter by optional query parameters:
        /// Usage examples:
        ///   - /api/cereal?Name=All-Bran
        ///   - /api/cereal?Mfr=K&CaloriesMin=70&CaloriesMax=120
        ///   - /api/cereal?FatMax=5&SodiumMin=100
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] CerealQueryParams queryParams,
                           string? sortBy = null, bool sortDescending = false)
        {
            _logger.LogTrace("GetAll called with queryParams: {@QueryParams}, sortBy: {SortBy}, sortDescending: {SortDescending}", queryParams, sortBy, sortDescending);

            try
            {
                var query = _context.Cereals.AsQueryable();

                // 1. Partial match (case-insensitive)
                if (!string.IsNullOrWhiteSpace(queryParams.Name))
                {
                    _logger.LogDebug("Filtering cereals by Name containing: {Name}", queryParams.Name);
                    query = query.Where(c => c.Name.ToLower().Contains(queryParams.Name.ToLower()));
                }

                // 2. Exact match (case-insensitive)
                if (!string.IsNullOrWhiteSpace(queryParams.Mfr))
                {
                    _logger.LogDebug("Filtering cereals by Manufacturer: {Mfr}", queryParams.Mfr);
                    query = query.Where(c => c.Mfr.ToLower() == queryParams.Mfr.ToLower());
                }

                if (!string.IsNullOrWhiteSpace(queryParams.Type))
                {
                    _logger.LogDebug("Filtering cereals by Type: {Type}", queryParams.Type);
                    query = query.Where(c => c.Type.ToLower() == queryParams.Type.ToLower());
                }

                // 3. Range-based filters for numeric fields
                if (queryParams.CaloriesMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Calories >= {CaloriesMin}", queryParams.CaloriesMin.Value);
                    query = query.Where(c => c.Calories >= queryParams.CaloriesMin.Value);
                }
                if (queryParams.CaloriesMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Calories <= {CaloriesMax}", queryParams.CaloriesMax.Value);
                    query = query.Where(c => c.Calories <= queryParams.CaloriesMax.Value);
                }

                if (queryParams.ProteinMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Protein >= {ProteinMin}", queryParams.ProteinMin.Value);
                    query = query.Where(c => c.Protein >= queryParams.ProteinMin.Value);
                }
                if (queryParams.ProteinMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Protein <= {ProteinMax}", queryParams.ProteinMax.Value);
                    query = query.Where(c => c.Protein <= queryParams.ProteinMax.Value);
                }

                if (queryParams.FatMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Fat >= {FatMin}", queryParams.FatMin.Value);
                    query = query.Where(c => c.Fat >= queryParams.FatMin.Value);
                }
                if (queryParams.FatMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Fat <= {FatMax}", queryParams.FatMax.Value);
                    query = query.Where(c => c.Fat <= queryParams.FatMax.Value);
                }

                if (queryParams.SodiumMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Sodium >= {SodiumMin}", queryParams.SodiumMin.Value);
                    query = query.Where(c => c.Sodium >= queryParams.SodiumMin.Value);
                }
                if (queryParams.SodiumMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Sodium <= {SodiumMax}", queryParams.SodiumMax.Value);
                    query = query.Where(c => c.Sodium <= queryParams.SodiumMax.Value);
                }

                if (queryParams.FiberMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Fiber >= {FiberMin}", queryParams.FiberMin.Value);
                    query = query.Where(c => c.Fiber >= queryParams.FiberMin.Value);
                }
                if (queryParams.FiberMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Fiber <= {FiberMax}", queryParams.FiberMax.Value);
                    query = query.Where(c => c.Fiber <= queryParams.FiberMax.Value);
                }

                if (queryParams.CarboMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Carbohydrates >= {CarboMin}", queryParams.CarboMin.Value);
                    query = query.Where(c => c.Carbo >= queryParams.CarboMin.Value);
                }
                if (queryParams.CarboMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Carbohydrates <= {CarboMax}", queryParams.CarboMax.Value);
                    query = query.Where(c => c.Carbo <= queryParams.CarboMax.Value);
                }

                if (queryParams.SugarsMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Sugars >= {SugarsMin}", queryParams.SugarsMin.Value);
                    query = query.Where(c => c.Sugars >= queryParams.SugarsMin.Value);
                }
                if (queryParams.SugarsMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Sugars <= {SugarsMax}", queryParams.SugarsMax.Value);
                    query = query.Where(c => c.Sugars <= queryParams.SugarsMax.Value);
                }

                if (queryParams.RatingMin.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Rating >= {RatingMin}", queryParams.RatingMin.Value);
                    query = query.Where(c => c.Rating >= queryParams.RatingMin.Value);
                }
                if (queryParams.RatingMax.HasValue)
                {
                    _logger.LogDebug("Filtering cereals with Rating <= {RatingMax}", queryParams.RatingMax.Value);
                    query = query.Where(c => c.Rating <= queryParams.RatingMax.Value);
                }

                // --- Sorting ---
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    _logger.LogDebug("Sorting cereals by {SortBy}, Descending: {SortDescending}", sortBy, sortDescending);

                    query = sortBy.ToLower() switch
                    {
                        "name" => sortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                        "mfr" => sortDescending ? query.OrderByDescending(c => c.Mfr) : query.OrderBy(c => c.Mfr),
                        "type" => sortDescending ? query.OrderByDescending(c => c.Type) : query.OrderBy(c => c.Type),
                        "calories" => sortDescending ? query.OrderByDescending(c => c.Calories) : query.OrderBy(c => c.Calories),
                        "protein" => sortDescending ? query.OrderByDescending(c => c.Protein) : query.OrderBy(c => c.Protein),
                        "fat" => sortDescending ? query.OrderByDescending(c => c.Fat) : query.OrderBy(c => c.Fat),
                        "sodium" => sortDescending ? query.OrderByDescending(c => c.Sodium) : query.OrderBy(c => c.Sodium),
                        "fiber" => sortDescending ? query.OrderByDescending(c => c.Fiber) : query.OrderBy(c => c.Fiber),
                        "carbo" => sortDescending ? query.OrderByDescending(c => c.Carbo) : query.OrderBy(c => c.Carbo),
                        "sugars" => sortDescending ? query.OrderByDescending(c => c.Sugars) : query.OrderBy(c => c.Sugars),
                        "rating" => sortDescending ? query.OrderByDescending(c => c.Rating) : query.OrderBy(c => c.Rating),
                        _ => query.OrderBy(c => c.Id), // Default sorting
                    };
                }

                // Execute query and get results
                var results = query.ToList();

                if (results.Count == 0)
                {
                    _logger.LogWarning("No cereals found matching the filters provided.");
                }
                else
                {
                    _logger.LogInformation("Retrieved {Count} cereals from the database.", results.Count);
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cereals from the database.");
                return StatusCode(500, "Internal server error.");
            }
        }


        /// <summary>
        /// GET a cereal by ID
        /// e.g. /api/cereal/5
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            _logger.LogTrace("GetById called with ID: {Id}", id);

            try
            {
                var cereal = _context.Cereals.Find(id);
                if (cereal == null)
                {
                    _logger.LogWarning("Cereal with ID {Id} not found.", id);
                    return NotFound($"Cereal with ID {id} not found.");
                }

                _logger.LogInformation("Cereal with ID {Id} retrieved successfully.", id);
                return Ok(cereal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cereal with ID {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// POST a new cereal or update an existing cereal if an ID is provided and already exists.
        /// </summary>
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] Cereal newCereal)
        {
            _logger.LogTrace("Create method called with data: {@NewCereal}", newCereal);

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized user attempted to create a cereal.");
                return Unauthorized();
            }

            try
            {
                if (newCereal == null)
                {
                    _logger.LogError("Cereal data is null.");
                    return BadRequest("Cereal data is null.");
                }

                // CASE 1: Create a new cereal if no ID is provided
                if (newCereal.Id == 0)
                {
                    _context.Cereals.Add(newCereal);
                    _context.SaveChanges();

                    _logger.LogInformation("Cereal created successfully with ID {Id}.", newCereal.Id);
                    return CreatedAtAction(nameof(GetById), new { id = newCereal.Id }, newCereal);
                }

                // CASE 2: Check if the cereal exists before updating
                var existingCereal = _context.Cereals.Find(newCereal.Id);

                if (existingCereal == null)
                {
                    _logger.LogWarning("Cereal with ID {Id} not found for update.", newCereal.Id);
                    return BadRequest($"Cereal with ID {newCereal.Id} does not exist. ID cannot be chosen manually for creation.");
                }

                // Update existing cereal
                existingCereal.Name = newCereal.Name;
                existingCereal.Mfr = newCereal.Mfr;
                existingCereal.Type = newCereal.Type;
                existingCereal.Calories = newCereal.Calories;
                existingCereal.Protein = newCereal.Protein;
                existingCereal.Fat = newCereal.Fat;
                existingCereal.Sodium = newCereal.Sodium;
                existingCereal.Fiber = newCereal.Fiber;
                existingCereal.Carbo = newCereal.Carbo;
                existingCereal.Sugars = newCereal.Sugars;
                existingCereal.Potass = newCereal.Potass;
                existingCereal.Vitamins = newCereal.Vitamins;
                existingCereal.Shelf = newCereal.Shelf;
                existingCereal.Weight = newCereal.Weight;
                existingCereal.Cups = newCereal.Cups;
                existingCereal.Rating = newCereal.Rating;
                existingCereal.ImagePath = newCereal.ImagePath;

                _context.Entry(existingCereal).State = EntityState.Modified;
                _context.SaveChanges();

                _logger.LogInformation("Cereal with ID {Id} updated successfully.", newCereal.Id);
                return Ok(existingCereal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating cereal.");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// PUT update an existing cereal by ID.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] Cereal updatedCereal)
        {
            _logger.LogTrace("Update method called for ID {Id} with data: {@UpdatedCereal}", id, updatedCereal);

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized user attempted to update a cereal.");
                return Unauthorized();
            }

            try
            {
                if (updatedCereal == null || id != updatedCereal.Id)
                {
                    _logger.LogError("Update failed due to invalid data or ID mismatch. ID: {Id}", id);
                    return BadRequest("Cereal data is invalid or ID mismatch.");
                }

                var existingCereal = _context.Cereals.Find(id);
                if (existingCereal == null)
                {
                    _logger.LogWarning("Cereal with ID {Id} not found for update.", id);
                    return NotFound($"Cereal with ID {id} not found.");
                }

                // Update the fields you want to allow changes to
                existingCereal.Name = updatedCereal.Name;
                existingCereal.Mfr = updatedCereal.Mfr;
                existingCereal.Type = updatedCereal.Type;
                existingCereal.Calories = updatedCereal.Calories;
                existingCereal.Protein = updatedCereal.Protein;
                existingCereal.Fat = updatedCereal.Fat;
                existingCereal.Sodium = updatedCereal.Sodium;
                existingCereal.Fiber = updatedCereal.Fiber;
                existingCereal.Carbo = updatedCereal.Carbo;
                existingCereal.Sugars = updatedCereal.Sugars;
                existingCereal.Potass = updatedCereal.Potass;
                existingCereal.Vitamins = updatedCereal.Vitamins;
                existingCereal.Shelf = updatedCereal.Shelf;
                existingCereal.Weight = updatedCereal.Weight;
                existingCereal.Cups = updatedCereal.Cups;
                existingCereal.Rating = updatedCereal.Rating;
                existingCereal.ImagePath = updatedCereal.ImagePath;

                _context.Entry(existingCereal).State = EntityState.Modified;
                _context.SaveChanges();

                _logger.LogInformation("Cereal with ID {Id} updated successfully.", id);
                return Ok(existingCereal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cereal with ID {Id}", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// DELETE a cereal by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            _logger.LogTrace("Delete method called for ID: {Id}", id);

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized user attempted to delete a cereal.");
                return Unauthorized();
            }

            try
            {
                var cereal = _context.Cereals.Find(id);
                if (cereal == null)
                {
                    _logger.LogWarning("Cereal with ID {Id} not found for deletion.", id);
                    return NotFound($"Cereal with ID {id} not found.");
                }

                _context.Cereals.Remove(cereal);
                _context.SaveChanges();

                _logger.LogInformation("Cereal with ID {Id} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cereal with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// DELETE all cereals from the database.
        /// </summary>
        [HttpDelete("all")]
        [Authorize]
        public IActionResult DeleteAll()
        {
            _logger.LogTrace("DeleteAll method called.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized user attempted to delete all cereals.");
                return Unauthorized();
            }

            try
            {
                var allCereals = _context.Cereals.ToList();
                if (!allCereals.Any())
                {
                    _logger.LogWarning("No cereals found for deletion.");
                    return NotFound("No cereals found in the database.");
                }

                _context.Cereals.RemoveRange(allCereals);
                _context.SaveChanges();

                _logger.LogInformation("All cereals deleted successfully. Count: {Count}", allCereals.Count);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all cereals.");
                return StatusCode(500, "Internal server error.");
            }
        }


        /// <summary>
        /// GET an image by cereal ID.
        /// Dynamically detects image format (JPG, PNG).
        /// Returns the cereal-specific image if available, otherwise the placeholder.
        /// If neither exists, returns 404.
        /// </summary>
        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public IActionResult GetImage(int id)
        {
            _logger.LogTrace("GetImage called for ID: {Id}", id);

            var cereal = _context.Cereals.Find(id);

            if (cereal == null)
            {
                _logger.LogWarning("Cereal with ID {Id} not found.", id);
                return NotFound($"Cereal with ID {id} not found.");
            }

            string? cerealImagePath = null;

            // 1. Check if the cereal has an existing ImagePath in the database
            if (!string.IsNullOrEmpty(cereal.ImagePath))
            {
                _logger.LogDebug("Checking for image at path: {ImagePath}", cereal.ImagePath);
                cerealImagePath = FindExistingImage(cereal.ImagePath);
            }

            // 2. If a valid cereal image exists, return it
            if (cerealImagePath != null)
            {
                _logger.LogInformation("Returning image for cereal ID {Id}: {ImagePath}", id, cerealImagePath);
                return File(System.IO.File.ReadAllBytes(cerealImagePath), GetMimeType(cerealImagePath));
            }

            // 3. Fallback: Check for the universal placeholder image
            string? placeholderPath = FindExistingImage("Data/Images/placeholder");

            if (placeholderPath != null)
            {
                _logger.LogDebug("Returning placeholder image for cereal ID {Id}.", id);
                return File(System.IO.File.ReadAllBytes(placeholderPath), GetMimeType(placeholderPath));
            }

            // 4. If neither exists, return 404
            _logger.LogWarning("No image found for cereal ID {Id}.", id);
            return NotFound("Image not found.");
        }

        /// <summary>
        /// Searches for an existing image file, checking multiple extensions.
        /// Returns the valid image path if found, otherwise null.
        /// </summary>
        private string? FindExistingImage(string basePath)
        {
            _logger.LogTrace("FindExistingImage called for basePath: {BasePath}", basePath);

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            // Check if the given path already has an extension (avoid double appending)
            string fileExtension = Path.GetExtension(basePath);
            if (!string.IsNullOrEmpty(fileExtension))
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), basePath);
                if (System.IO.File.Exists(fullPath))
                {
                    _logger.LogDebug("Found image with direct path: {FullPath}", fullPath);
                    return fullPath;
                }
            }

            // If no extension, try common image formats
            foreach (var ext in allowedExtensions)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"{basePath}{ext}");
                if (System.IO.File.Exists(fullPath))
                {
                    _logger.LogDebug("Found image with extension {Ext}: {FullPath}", ext, fullPath);
                    return fullPath;
                }
            }

            _logger.LogWarning("No image found for basePath: {BasePath}", basePath);
            return null;
        }

        /// <summary>
        /// Determines the MIME type of an image based on its file extension.
        /// It tells the browser or client what type of file is being sent (e.g., "image/png" or "image/jpeg").
        /// </summary>
        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string mimeType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            _logger.LogTrace("GetMimeType called for file: {FilePath}, resolved MIME type: {MimeType}", filePath, mimeType);
            return mimeType;
        }

    }
}
