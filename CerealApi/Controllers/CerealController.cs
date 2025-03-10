using CerealApi.Data;
using CerealApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace CerealApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CerealController : ControllerBase
    {
        private readonly CerealContext _context;

        public CerealController(CerealContext context)
        {
            _context = context;
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
        public IActionResult GetAll([FromQuery] CerealQueryParams queryParams)
        {
            try
            {
                var query = _context.Cereals.AsQueryable();

                // 1. Partial match (case-insensitive)

                // Name
                if (!string.IsNullOrWhiteSpace(queryParams.Name))
                {
                    var nameLower = queryParams.Name.ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(nameLower));
                }

                // 2. Exact match (case-insensitive)

                //Manufacturer
                if (!string.IsNullOrWhiteSpace(queryParams.Mfr))
                {
                    var mfrLower = queryParams.Mfr.ToLower();
                    query = query.Where(c => c.Mfr.ToLower() == mfrLower);
                }

                // C/H , Cold/Hotg
                if (!string.IsNullOrWhiteSpace(queryParams.Type))
                {
                    var typeLower = queryParams.Type.ToLower();
                    query = query.Where(c => c.Type.ToLower() == typeLower);
                }

                // 3. Range-based filters for numeric fields

                // Calories
                if (queryParams.CaloriesMin.HasValue)
                {
                    query = query.Where(c => c.Calories >= queryParams.CaloriesMin.Value);
                }
                if (queryParams.CaloriesMax.HasValue)
                {
                    query = query.Where(c => c.Calories <= queryParams.CaloriesMax.Value);
                }

                // Protein
                if (queryParams.ProteinMin.HasValue)
                {
                    query = query.Where(c => c.Protein >= queryParams.ProteinMin.Value);
                }
                if (queryParams.ProteinMax.HasValue)
                {
                    query = query.Where(c => c.Protein <= queryParams.ProteinMax.Value);
                }

                // Fat
                if (queryParams.FatMin.HasValue)
                {
                    query = query.Where(c => c.Fat >= queryParams.FatMin.Value);
                }
                if (queryParams.FatMax.HasValue)
                {
                    query = query.Where(c => c.Fat <= queryParams.FatMax.Value);
                }

                // Sodium
                if (queryParams.SodiumMin.HasValue)
                {
                    query = query.Where(c => c.Sodium >= queryParams.SodiumMin.Value);
                }
                if (queryParams.SodiumMax.HasValue)
                {
                    query = query.Where(c => c.Sodium <= queryParams.SodiumMax.Value);
                }

                // Fiber
                if (queryParams.FiberMin.HasValue)
                {
                    query = query.Where(c => c.Fiber >= queryParams.FiberMin.Value);
                }
                if (queryParams.FiberMax.HasValue)
                {
                    query = query.Where(c => c.Fiber <= queryParams.FiberMax.Value);
                }

                // Carbo
                if (queryParams.CarboMin.HasValue)
                {
                    query = query.Where(c => c.Carbo >= queryParams.CarboMin.Value);
                }
                if (queryParams.CarboMax.HasValue)
                {
                    query = query.Where(c => c.Carbo <= queryParams.CarboMax.Value);
                }

                // Sugars
                if (queryParams.SugarsMin.HasValue)
                {
                    query = query.Where(c => c.Sugars >= queryParams.SugarsMin.Value);
                }
                if (queryParams.SugarsMax.HasValue)
                {
                    query = query.Where(c => c.Sugars <= queryParams.SugarsMax.Value);
                }

                // Potass
                if (queryParams.PotassMin.HasValue)
                {
                    query = query.Where(c => c.Potass >= queryParams.PotassMin.Value);
                }
                if (queryParams.PotassMax.HasValue)
                {
                    query = query.Where(c => c.Potass <= queryParams.PotassMax.Value);
                }

                // Vitamins
                if (queryParams.VitaminsMin.HasValue)
                {
                    query = query.Where(c => c.Vitamins >= queryParams.VitaminsMin.Value);
                }
                if (queryParams.VitaminsMax.HasValue)
                {
                    query = query.Where(c => c.Vitamins <= queryParams.VitaminsMax.Value);
                }

                // Shelf
                if (queryParams.ShelfMin.HasValue)
                {
                    query = query.Where(c => c.Shelf >= queryParams.ShelfMin.Value);
                }
                if (queryParams.ShelfMax.HasValue)
                {
                    query = query.Where(c => c.Shelf <= queryParams.ShelfMax.Value);
                }

                // Weight
                if (queryParams.WeightMin.HasValue)
                {
                    query = query.Where(c => c.Weight >= queryParams.WeightMin.Value);
                }
                if (queryParams.WeightMax.HasValue)
                {
                    query = query.Where(c => c.Weight <= queryParams.WeightMax.Value);
                }

                // Cups
                if (queryParams.CupsMin.HasValue)
                {
                    query = query.Where(c => c.Cups >= queryParams.CupsMin.Value);
                }
                if (queryParams.CupsMax.HasValue)
                {
                    query = query.Where(c => c.Cups <= queryParams.CupsMax.Value);
                }

                // Rating
                if (queryParams.RatingMin.HasValue)
                {
                    query = query.Where(c => c.Rating >= queryParams.RatingMin.Value);
                }
                if (queryParams.RatingMax.HasValue)
                {
                    query = query.Where(c => c.Rating <= queryParams.RatingMax.Value);
                }

                // Finalize and execute the query
                var results = query.ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log ex if you have a logging mechanism
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            try
            {
                var cereal = _context.Cereals.Find(id);
                if (cereal == null)
                {
                    return NotFound($"Cereal with ID {id} not found.");
                }
                return Ok(cereal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// POST a new cereal or update an existing cereal if an ID is provided and already exists.
        /// </summary>
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] Cereal newCereal)
        {
            // Check if the user is authenticated (unit test workaround)
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                if (newCereal == null)
                {
                    return BadRequest("Cereal data is null.");
                }

                // CASE 1: If no ID is provided (Id == 0), create a new cereal
                if (newCereal.Id == 0)
                {
                    _context.Cereals.Add(newCereal);
                    _context.SaveChanges();

                    return CreatedAtAction(nameof(GetById), new { id = newCereal.Id }, newCereal);
                }

                // CASE 2: If the user provides an ID (Id != 0),
                //         check if that cereal exists in the database
                var existingCereal = _context.Cereals.Find(newCereal.Id);

                // If it doesn't exist, return an error (can't pick arbitrary IDs for new cereals)
                if (existingCereal == null)
                {
                    return BadRequest($"Cereal with ID {newCereal.Id} does not exist. ID cannot be chosen manually for creation.");
                }

                // If it exists, update the existing record
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

                return Ok(existingCereal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// PUT update an existing cereal by ID.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] Cereal updatedCereal)
        {

            // Check if the user is authenticated (unit test workaround)
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                if (updatedCereal == null || id != updatedCereal.Id)
                {
                    return BadRequest("Cereal data is invalid or ID mismatch.");
                }

                var existing = _context.Cereals.Find(id);
                if (existing == null)
                {
                    return NotFound($"Cereal with ID {id} not found.");
                }

                // Update the fields you want to allow changes to
                existing.Name = updatedCereal.Name;
                existing.Mfr = updatedCereal.Mfr;
                existing.Type = updatedCereal.Type;
                existing.Calories = updatedCereal.Calories;
                existing.Protein = updatedCereal.Protein;
                existing.Fat = updatedCereal.Fat;
                existing.Sodium = updatedCereal.Sodium;
                existing.Fiber = updatedCereal.Fiber;
                existing.Carbo = updatedCereal.Carbo;
                existing.Sugars = updatedCereal.Sugars;
                existing.Potass = updatedCereal.Potass;
                existing.Vitamins = updatedCereal.Vitamins;
                existing.Shelf = updatedCereal.Shelf;
                existing.Weight = updatedCereal.Weight;
                existing.Cups = updatedCereal.Cups;
                existing.Rating = updatedCereal.Rating;
                existing.ImagePath = updatedCereal.ImagePath;

                _context.Entry(existing).State = EntityState.Modified;
                _context.SaveChanges();

                return Ok(existing);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE an existing cereal by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {

            // Check if the user is authenticated (unit test workaround)
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var cereal = _context.Cereals.Find(id);
                if (cereal == null)
                {
                    return NotFound($"Cereal with ID {id} not found.");
                }

                _context.Cereals.Remove(cereal);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            var cereal = _context.Cereals.Find(id);

            if (cereal == null)
            {
                return NotFound($"Cereal with ID {id} not found.");
            }

            string? cerealImagePath = null;

            // 1. Check if the cereal has an existing ImagePath in the database
            if (!string.IsNullOrEmpty(cereal.ImagePath))
            {
                cerealImagePath = FindExistingImage(cereal.ImagePath);
            }

            // 2. If a valid cereal image exists, return it
            if (cerealImagePath != null)
            {
                return File(System.IO.File.ReadAllBytes(cerealImagePath), GetMimeType(cerealImagePath));
            }

            // 3. Fallback: Check for the universal placeholder image
            string? placeholderPath = FindExistingImage("Data/Images/placeholder");

            if (placeholderPath != null)
            {
                return File(System.IO.File.ReadAllBytes(placeholderPath), GetMimeType(placeholderPath));
            }

            // 4. If neither exists, return 404
            return NotFound("Image not found.");
        }

        /// <summary>
        /// Searches for an existing image file, checking multiple extensions.
        /// Returns the valid image path if found, otherwise null.
        /// </summary>
        private string? FindExistingImage(string basePath)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            // Check if the given path already has an extension (avoid double appending)
            string fileExtension = Path.GetExtension(basePath);
            if (!string.IsNullOrEmpty(fileExtension))
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), basePath);
                return System.IO.File.Exists(fullPath) ? fullPath : null;
            }

            // If no extension, try common image formats
            foreach (var ext in allowedExtensions)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"{basePath}{ext}");
                if (System.IO.File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines the MIME type of an image based on its file extension.
        /// It tells the browser or client what type of file is being sent (e.g., "image/png" or "image/jpeg").
        /// </summary>
        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }

    }
}
