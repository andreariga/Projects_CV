namespace PokeAPI.Model
{
    public class Weakness
    {
        public int WeaknessId { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public int DatasId { get; set; }
        public Datas? Datas { get; set; }
    }
}
