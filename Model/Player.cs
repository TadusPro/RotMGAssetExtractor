using RotMGAssetExtractor.ModelHelpers;

namespace RotMGAssetExtractor.Model
{
    public class Player : Object
    {
        public string Description { get; set; }
        public AnimatedTexture AnimatedTexture { get; set; }
        public string HitSound { get; set; }
        public string DeathSound { get; set; }
        public Stat MaxHitPoints { get; set; }
        public Stat MaxMagicPoints { get; set; }
        public Stat Attack { get; set; }
        public Stat Defense { get; set; }
        public Stat Speed { get; set; }
        public Stat Dexterity { get; set; }
        public Stat HpRegen { get; set; }
        public Stat MpRegen { get; set; }
        public int UnlockCost { get; set; }
        public double BloodProb { get; set; }
        public int[] SlotTypes { get; set; }
        public LevelIncrease[] LevelIncrease { get; set; }


    }
}
