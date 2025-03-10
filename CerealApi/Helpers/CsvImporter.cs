using System;
using System.IO;
using System.Linq;
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

            for (int i = 2; i < lines.Length; i++) // Start at index 2 to skip headers
            {
                var row = lines[i].Split(';'); // Updated to match CSV delimiter

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
                    continue; // Skip adding a duplicate cereal
                }

                // Create a new cereal entry
                try
                {
                    var cereal = new Cereal
                    {
                        Name = cerealName,
                        Mfr = row[1].Trim(),
                        Type = row[2].Trim(),
                        Calories = int.Parse(row[3]),
                        Protein = int.Parse(row[4]),
                        Fat = int.Parse(row[5]),
                        Sodium = int.Parse(row[6]),
                        Fiber = float.Parse(row[7]),
                        Carbo = float.Parse(row[8]),
                        Sugars = int.Parse(row[9]),
                        Potass = int.Parse(row[10]),
                        Vitamins = int.Parse(row[11]),
                        Shelf = int.Parse(row[12]),
                        Weight = float.Parse(row[13]),
                        Cups = float.Parse(row[14]),
                        Rating = float.Parse(row[15]),

                        // Store the found image path if available
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
