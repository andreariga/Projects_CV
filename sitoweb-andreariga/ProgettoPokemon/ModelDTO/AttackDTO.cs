using PokeAPI.Model;

namespace PokeAPI.ModelDTO
{
    public class AttackDTO
    {
        public int AttackId { get; set; }
        public string? Name { get; set; }
        public List<string> Cost { get; set; } = null!;
        public int? ConvertedEnergyCost { get; set; }
        public string? Damage { get; set; }
        public string? Text { get; set; }
        public int DatasId { get; set; }
        public AttackDTO() { }
        public AttackDTO(Attack attack)
        {
            AttackId = attack.AttackId;
            Name = attack.Name;
            Cost = attack.Cost;
            ConvertedEnergyCost = attack.ConvertedEnergyCost;
            Damage = attack.Damage;
            Text = attack.Text;
            DatasId = attack.DatasId;
        }
    }
}
