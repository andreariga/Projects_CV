using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class SetDTO
    {
        public int SetId { get; set; }
        public string? Identificatore { get; set; }
        public string? Name { get; set; }
        public string? Series { get; set; }
        public int? PrintedTotal { get; set; }
        public int? Total { get; set; }
        public string? PtcgoCode { get; set; }
        public string? ReleaseDate { get; set; }
        public string? UpdatedAt { get; set; }
        public int? ImagesId { get; set; }
        public SetDTO() { }
        public SetDTO(Set set)
        {
            SetId = set.SetId;
            Identificatore = set.Identificatore;
            Name = set.Name;
            Series = set.Series;
            PrintedTotal = set.PrintedTotal;
            Total = set.Total;
            PtcgoCode = set.PtcgoCode;
            ReleaseDate = set.ReleaseDate;
            UpdatedAt = set.UpdatedAt;
            ImagesId = set.ImagesId;
        }
    }
}
