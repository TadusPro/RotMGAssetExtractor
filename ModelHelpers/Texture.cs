namespace RotMGAssetExtractor.ModelHelpers
{
    public class Texture : ITexture
    {
        public string File { get; set; }
        public int Index { get; set; }
        public int xOffset { get; set; }
        public int yOffset { get; set; }
    }
}
