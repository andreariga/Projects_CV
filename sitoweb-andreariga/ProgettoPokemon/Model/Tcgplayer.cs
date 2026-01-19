namespace PokeAPI.Model
{
    public class Tcgplayer
    {
        public int TcgplayerId { get; set; }
        public string? Url { get; set; }
        public string? UpdatedAt { get; set; }
        public int? PricesId { get; set; }
        public Prices? Prices { get; set; }
    }
}
