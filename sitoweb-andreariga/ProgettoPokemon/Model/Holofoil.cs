namespace PokeAPI.Model
{
    public class Holofoil
    {
        public int HolofoilId { get; set; }
        public double? Low { get; set; }
        public double? Mid { get; set; }
        public double? High { get; set; }
        public double? Market { get; set; }
        public double? DirectLow { get; set; }
        public int? PricesId { get; set; }
        public Prices? Prices { get; set; }
    }
}
