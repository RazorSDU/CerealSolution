using System;
using System.IO;
using System.Linq;
using System.Globalization;
using CerealApi.Models;
using CerealApi.Data;

namespace CerealApi.Helpers
{
    public static class CsvImporter
    {
        public static void ImportCereals(string csvPath, string imagesFolderPath, CerealContext context)
        {
            // Read all lines from the CSV file
            var lines = File.ReadAllLines(csvPath);

            int addedCount = 0;  // Track newly added cereals
            int updatedCount = 0; // Track cereals that had missing images and got updated

            // Start at index 2 to skip the two header lines
            for (int i = 2; i < lines.Length; i++)
            {
                var row = lines[i].Split(';');

                // Ensure the row has exactly 16 columns before processing
                if (row.Length != 16)
                {
                    Console.WriteLine($"Skipping malformed row at line {i + 1}: {lines[i]}");
                    continue;
                }

                var cerealName = row[0].Trim(); // Extract Name column

                // Check if a cereal with this name already exists in the database
                var existingCereal = context.Cereals.FirstOrDefault(c => c.Name == cerealName);

                // Find the correct image path (JPG or PNG)
                string? imageFilePath = FindImageWithExtension(imagesFolderPath, cerealName);

                if (existingCereal != null)
                {
                    // If the cereal exists but has no image, update it
                    if (existingCereal.ImagePath == null && imageFilePath != null)
                    {
                        existingCereal.ImagePath = imageFilePath;
                        updatedCount++;
                    }
                    // Skip adding a duplicate cereal
                    continue;
                }

                // Attempt to parse and create a new cereal entry
                try
                {
                    int calories = int.Parse(row[3]);
                    int protein = int.Parse(row[4]);
                    int fat = int.Parse(row[5]);
                    int sodium = int.Parse(row[6]);
                    float fiber = float.Parse(row[7], CultureInfo.InvariantCulture);
                    float carbo = float.Parse(row[8], CultureInfo.InvariantCulture);
                    int sugars = int.Parse(row[9]);

                    // Potassium fix: if the CSV has -1, use 0
                    int potassRaw = int.Parse(row[10]);
                    int potass = (potassRaw < 0) ? 0 : potassRaw;

                    int vitamins = int.Parse(row[11]);
                    int shelf = int.Parse(row[12]);
                    float weight = float.Parse(row[13], CultureInfo.InvariantCulture);
                    float cups = float.Parse(row[14], CultureInfo.InvariantCulture);

                    // Fix the rating string so that only the first period is treated as decimal,
                    // removing any subsequent periods
                    string rawRating = row[15].Trim();
                    rawRating = FixRatingDecimalFormat(rawRating);

                    // Parse the corrected float
                    float originalRating = float.Parse(rawRating, CultureInfo.InvariantCulture);

                    // Convert from approximately 0–100 scale to 0–5 stars
                    float fiveStarRating = originalRating / 20f;
                    // Optionally round to 2 decimals
                    fiveStarRating = (float)Math.Round(fiveStarRating, 2);

                    var cereal = new Cereal
                    {
                        Name = cerealName,
                        Mfr = row[1].Trim(),
                        Type = row[2].Trim(),
                        Calories = calories,
                        Protein = protein,
                        Fat = fat,
                        Sodium = sodium,
                        Fiber = fiber,
                        Carbo = carbo,
                        Sugars = sugars,
                        Potass = potass,
                        Vitamins = vitamins,
                        Shelf = shelf,
                        Weight = weight,
                        Cups = cups,
                        Rating = fiveStarRating,
                        ImagePath = imageFilePath
                    };

                    // Add new cereal record to DB
                    context.Cereals.Add(cereal);
                    addedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row {i + 1}: {ex.Message}");
                }
            }

            // Save all records to the database
            if (addedCount > 0 || updatedCount > 0)
            {
                context.SaveChanges();
                Console.WriteLine($"Import complete. {addedCount} new cereals added. {updatedCount} cereals updated with image paths.");
            }
            else
            {
                Console.WriteLine("No new cereals added or updated.");
            }
        }

        /// <summary>
        /// Interprets the *first* period as the decimal separator and removes
        /// all subsequent periods. Example:
        ///   "93.704.912" => "93.704912" => parse => ~93.704912
        /// </summary>
        private static string FixRatingDecimalFormat(string ratingStr)
        {
            // Find the index of the first '.'
            int firstDotIndex = ratingStr.IndexOf('.');
            if (firstDotIndex == -1)
            {
                // No '.' at all; just return as-is
                return ratingStr;
            }

            // Everything before the first '.'
            string prefix = ratingStr.Substring(0, firstDotIndex);
            // The rest (everything after the first '.'), removing *all* '.' from it
            string suffix = ratingStr.Substring(firstDotIndex + 1).Replace(".", "");

            // Combine with exactly one '.' in the middle
            return prefix + "." + suffix;
        }

        /// <summary>
        /// Searches for an image with different possible file extensions.
        /// Returns the relative image path if found, otherwise null.
        /// </summary>
        private static string? FindImageWithExtension(string folderPath, string cerealName)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            foreach (var ext in allowedExtensions)
            {
                string imagePath = Path.Combine(folderPath, $"{cerealName}{ext}");

                if (File.Exists(imagePath))
                {
                    return Path.Combine("Data", "Images", $"{cerealName}{ext}"); // Store relative path
                }
            }

            return null; // No matching image found
        }
    }
}
