using System;
using System.Collections.Generic;

namespace RotMGAssetExtractor.Model
{
    public class Portal
    {
        public int type { get; set; }
        public string id { get; set; }
        public ModelHelpers.Texture Texture { get; set; }
        public ModelHelpers.AnimatedTexture AnimatedTexture { get; set; }
        public string DisplayId { get; set; }
        public string ShadowSize { get; set; }
        public string Animation { get; set; }
        public string Size { get; set; }
        public string AltTexture { get; set; }
        public string Color { get; set; }
        public string DungeonName { get; set; }
        public string DungeonPortal { get; set; }
        public string IntergamePortal { get; set; }
        public string TeleportingPortal { get; set; }

    }
}
