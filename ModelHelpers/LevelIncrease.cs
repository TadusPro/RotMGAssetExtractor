using System.Xml.Serialization;

namespace RotMGAssetExtractor.ModelHelpers
{
    public class LevelIncrease
    {
        [XmlAttribute("min")] public int Min { get; set; }
        [XmlAttribute("max")] public int Max { get; set; }

        // inner text → which stat this line belongs to
        [XmlText] public string StatName { get; set; }
    }

}
