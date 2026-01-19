using PokeAPI.Model;

namespace PokeAPI.Model
{
    public class Datas
    {
        public int DatasId { get; set; } 
        public string? Name { get; set; }
        public string? Supertype { get; set; }
        public string? Hp { get; set; }
        public string? Number { get; set; }
        public string? Artist { get; set; }
        public string? Rarity { get; set; }
        public int? ConvertedRetreatCost { get; set; }
        public int? SetId { get; set; }
        public Set? Set { get; set; }

        public int? LegalitiesId { get; set; }
        public Legalities? Legalities { get; set; }

        public int? ImagesId { get; set; }
        public Images? Images { get; set; }

        public int? TcgplayerId { get; set; }
        public Tcgplayer? Tcgplayer { get; set; }
        public List<Attack>? Attacks { get; set; }
        public List<Weakness>? Weaknesses { get; set; }
    }
}
