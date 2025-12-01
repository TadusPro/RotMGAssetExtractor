using System.Xml.Serialization;

namespace RotMGAssetExtractor.ModelHelpers
{
    public class Texture : ITexture
    {
        public string File { get; set; }
        public int Index { get; set; }
        public int xOffset { get; set; }
        public int yOffset { get; set; }
        [XmlIgnore]
        public bool xOffsetSpecified => xOffset != 0;

        [XmlIgnore]
        public bool yOffsetSpecified => yOffset != 0;
    }
}
