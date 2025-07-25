using System.Xml.Serialization;

namespace RotMGAssetExtractor.ModelHelpers
{
    public class Stat
    {
        [XmlAttribute("max")]
        public int Max { get; set; }

        [XmlText]
        public int Value { get; set; }
    }
}
