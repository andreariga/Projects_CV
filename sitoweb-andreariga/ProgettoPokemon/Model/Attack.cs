namespace PokeAPI.Model
{
    public class Attack
    {
        public int AttackId { get; set; }
        public string? Name { get; set; }
        public List<string> Cost { get; set; } = null!;
        public int? ConvertedEnergyCost { get; set; }
        public string? Damage { get; set; }
        public string? Text { get; set; }
        public int DatasId { get; set; }
        public Datas? Datas { get; set; }
    }
}
