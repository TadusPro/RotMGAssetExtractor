namespace RotMGAssetExtractor.Model
{
    public class PlayerStat
    {
        public int index { get; set; }
        public string id { get; set; } // idname
        public int reportEvery { get; set; }
        public bool dungeon { get; set; }
        public string displayName { get; set; }
        public string displayColor { get; set; }
        public bool displayOnDeath { get; set; }
        public string dungeonId { get; set; }
    }
}
