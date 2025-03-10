namespace CerealApi.Models
{
    public class CerealQueryParams
    {
        // String fields (Partial matches)
        public string? Name { get; set; }

        // String fields (exact matches)
        public string? Mfr { get; set; }
        public string? Type { get; set; }  // hot/cold

        // Example usage: ?Name=All-Bran, ?Mfr=K, ?Type=C

        // For numeric fields, define min & max
        // (Use nullable int for integer fields, nullable float for float fields)

        public int? CaloriesMin { get; set; }
        public int? CaloriesMax { get; set; }

        public int? ProteinMin { get; set; }
        public int? ProteinMax { get; set; }

        public int? FatMin { get; set; }
        public int? FatMax { get; set; }

        public int? SodiumMin { get; set; }
        public int? SodiumMax { get; set; }

        public float? FiberMin { get; set; }
        public float? FiberMax { get; set; }

        public float? CarboMin { get; set; }
        public float? CarboMax { get; set; }

        public int? SugarsMin { get; set; }
        public int? SugarsMax { get; set; }

        public int? PotassMin { get; set; }
        public int? PotassMax { get; set; }

        public int? VitaminsMin { get; set; }
        public int? VitaminsMax { get; set; }

        public int? ShelfMin { get; set; }
        public int? ShelfMax { get; set; }

        public float? WeightMin { get; set; }
        public float? WeightMax { get; set; }

        public float? CupsMin { get; set; }
        public float? CupsMax { get; set; }

        public float? RatingMin { get; set; }
        public float? RatingMax { get; set; }
    }
}
