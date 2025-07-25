using System.Xml.Serialization;

namespace RotMGAssetExtractor.ModelHelpers
{
    public class Condition
    {
        [XmlAttribute("threshold")]
        public int threshold { get; set; }
        
        [XmlAttribute("stat")]
        public string stat { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
