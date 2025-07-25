namespace RotMGAssetExtractor.Model
{
    public class Enchantment
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public string DisplayId { get; set; }
        public ModelHelpers.Texture Texture { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public string CompatibleWithItemLabels { get; set; }
        public string IncompatibleWithItemLabels { get; set; }
        public string IncompatibleWithItemIds { get; set; }
        public string EnchantmentLabels { get; set; }
        public string IncompatibleWithEnchantmentLabels { get; set; }
        public string Mutators { get; set; }
        public int PowerLevelAdd { get; set; }
        public int PowerLevelMult { get; set; }
    }
}
