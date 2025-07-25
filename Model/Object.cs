using RotMGAssetExtractor.ModelHelpers;

namespace RotMGAssetExtractor.Model
{
    public class Object
    {
        public string id { get; set; }
        public int type { get; set; }
        public string Class { get; set; }
        public Texture Texture { get; set; }
        public AnimatedTexture AnimatedTexture { get; set; }
    }
}
