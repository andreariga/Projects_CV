using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class TcgplayerDTO
    {
        public int TcgplayerId { get; set; }
        public string? Url { get; set; }
        public string? UpdatedAt { get; set; }
        public int? PricesId { get; set; }
        public TcgplayerDTO() { }
        public TcgplayerDTO(Tcgplayer tcgplayer)
        {
            TcgplayerId = tcgplayer.TcgplayerId;
            Url = tcgplayer.Url;
            UpdatedAt = tcgplayer.UpdatedAt;
            PricesId = tcgplayer.PricesId;
        }
    }
}
