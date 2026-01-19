using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class PricesDTO
    {
        public int PricesId { get; set; }
        public PricesDTO() { }
        public PricesDTO(Prices prices)
        {
            PricesId = prices.PricesId;
        }
    }
}
