using System.Collections.Generic;
using System.Xml.Serialization;

namespace RotMGAssetExtractor
{
    public class DecompiledSpriteSheet
    {
        [XmlArray("SpriteGroups")]
        [XmlArrayItem("SpriteGroup")]
        public List<SpriteGroup> SpriteGroups { get; set; } = new();
    }

    public class SpriteGroup
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "";

        [XmlElement("Sprite")]
        public List<SpriteInfo> Sprites { get; set; } = new();
    }

    public class SpriteInfo
    {
        [XmlAttribute("Index")]
        public int Index { get; set; }

        [XmlAttribute("AtlasId")]
        public int AtlasId { get; set; }

        [XmlAttribute("X")]
        public int X { get; set; }
        [XmlAttribute("Y")]
        public int Y { get; set; }
        [XmlAttribute("W")]
        public int W { get; set; }
        [XmlAttribute("H")]
        public int H { get; set; }
    }
}