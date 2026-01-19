using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class DatasDTO
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
        public int? LegalitiesId { get; set; }
        public int? ImagesId { get; set; }
        public int? TcgplayerId { get; set; }
        public DatasDTO() { }
        public DatasDTO(Datas datas)
        {
            DatasId = datas.DatasId;
            Name = datas.Name;
            Supertype = datas.Supertype;
            Hp = datas.Hp;
            Number = datas.Number;
            Artist = datas.Artist;
            Rarity = datas.Rarity;
            ConvertedRetreatCost = datas.ConvertedRetreatCost;
            SetId = datas.SetId;
            LegalitiesId = datas.LegalitiesId;
            ImagesId = datas.ImagesId;
            TcgplayerId = datas.TcgplayerId;
        }
    }
}
