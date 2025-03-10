namespace CerealApi.Models
{
    public class Cereal
    {
        public int Id { get; set; }  // Primary Key
        public string Name { get; set; } = string.Empty;
        public string Mfr { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // hot/cold
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Fat { get; set; }
        public int Sodium { get; set; }
        public float Fiber { get; set; }
        public float Carbo { get; set; }
        public int Sugars { get; set; }
        public int Potass { get; set; }
        public int Vitamins { get; set; }
        public int Shelf { get; set; }
        public float Weight { get; set; }
        public float Cups { get; set; }
        public float Rating { get; set; }
        public string? ImagePath { get; set; }
    }
}
