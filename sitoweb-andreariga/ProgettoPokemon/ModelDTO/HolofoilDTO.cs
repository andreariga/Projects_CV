using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class HolofoilDTO
    {
        public int HolofoilId { get; set; }
        public double? Low { get; set; }
        public double? Mid { get; set; }
        public double? High { get; set; }
        public double? Market { get; set; }
        public double? DirectLow { get; set; }
        public int? PricesId { get; set; }
        public HolofoilDTO() { }
        public HolofoilDTO(Holofoil holofoil)
        {
            HolofoilId = holofoil.HolofoilId;
            Low = holofoil.Low;
            Mid = holofoil.Mid;
            High = holofoil.High;
            Market = holofoil.Market;
            DirectLow = holofoil.DirectLow;
            PricesId = holofoil.PricesId;
        }
    }
}
