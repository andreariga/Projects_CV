using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class WeaknessDTO
    {
        public int WeaknessId { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public int DatasId { get; set; }
        public WeaknessDTO() { }
        public WeaknessDTO(Weakness weakness)
        {
            WeaknessId = weakness.WeaknessId;
            Type = weakness.Type;
            Value = weakness.Value;
            DatasId = weakness.DatasId;
        }
    }
}
