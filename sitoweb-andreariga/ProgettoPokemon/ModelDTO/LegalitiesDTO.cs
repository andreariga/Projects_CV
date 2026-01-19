using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class LegalitiesDTO
    {
        public int LegalitiesId { get; set; }
        public string? Unlimited { get; set; }
        public string? Expanded { get; set; }
        public LegalitiesDTO() { }
        public LegalitiesDTO(Legalities legalities)
        {
            LegalitiesId = legalities.LegalitiesId;
            Unlimited = legalities.Unlimited;
            Expanded = legalities.Expanded;
        }
    }
}
