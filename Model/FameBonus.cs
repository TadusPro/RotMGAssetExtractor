namespace RotMGAssetExtractor.Model
{
    public class FameBonus
    {
        public string id { get; set; } // idname
        public int code { get; set; } // kinda id
        public string DisplayGroup { get; set; }
        public string DisplayCategory { get; set; }
        public string DisplayName { get; set; }
        public int AbsoluteBonus { get; set; }
        public int Description { get; set; }
        public int ShortDisplayName { get; set; }
        public int MaxRepeatCount { get; set; }
        public bool Repeatable { get; set; }
        public float RelativeBonus { get; set; }
        public ModelHelpers.Condition[] Condition { get; set; }

    }
}
