namespace PokeAPI.Model
{
    public class Set
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
        public Images? Images { get; set; }
    }
}
