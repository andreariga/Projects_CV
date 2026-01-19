using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class ImagesDTO
    {
        public int ImagesId { get; set; }
        public string? Symbol { get; set; }
        public string? Logo { get; set; }
        public string? Small { get; set; }
        public string? Large { get; set; }
        public ImagesDTO() { }
        public ImagesDTO(Images images)
        {
            ImagesId = images.ImagesId;
            Symbol = images.Symbol;
            Logo = images.Logo;
            Small = images.Small;
            Large = images.Large;
        }
    }
}
