namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class Resources
    {
        public TextAsset spritesheet;
        public TextAsset manifest_json;
        public TextAsset manifest_xml;
        public List<TextAsset> assetTextAssets = new List<TextAsset>();
        public List<SpriteAtlas> assetSpriteAtlases = new List<SpriteAtlas>();
        public List<Texture2D> assetTexture2Ds = new List<Texture2D>();
        private readonly byte[]? _resSData;

        public Resources(byte[] assetsData, byte[]? resSData = null)
        {
            _resSData = resSData;
            try
            {
                FileHeader header = new FileHeader(assetsData);
                if (header.Type == FileHeader.AssetsFile)
                {
                    extractAssets(assetsData, header);
                }
            }
            catch (IOException ex)
            {
                // Handle or rethrow the exception as needed
                throw;
            }
        }

        public void extractAssets(byte[] data, FileHeader header)
        {
            SerializedFile sf = new SerializedFile(data, header);
            ParseAllResources(sf);
        }

        public void ParseAllResources(SerializedFile sf)
        {
            int totalObjects = sf.Objects.Count();
            for (int i = 0; i < totalObjects; i++)
            {
                var o = sf.Objects[i];

                switch (o.Type)
                {
                    case ClassIDType.TextAsset:
                        ParseTextAsset(o);
                        break;
                    case ClassIDType.SpriteAtlas:
                        ParseSpriteAtlas(o);
                        break;
                    case ClassIDType.Texture2D:
                        ParseTexture2D(o);
                        break;
                    // Other cases like GameObject, MonoBehaviour, etc., can be added here if needed.
                    default:
                        break;
                }
            }
        }

        private void ParseTextAsset(ObjectReader o)
        {
            TextAsset t = new TextAsset(o);
            switch (t.Name)
            {
                case "spritesheetf":
                    spritesheet = t;
                    break;
                case "manifest":
                    manifest_json = t;
                    break;
                case "assets_manifest":
                    manifest_xml = t;
                    break;
            }
            assetTextAssets.Add(t);
        }

        private void ParseSpriteAtlas(ObjectReader o)
        {
            SpriteAtlas s = new SpriteAtlas(o);
            assetSpriteAtlases.Add(s);
        }

        private void ParseTexture2D(ObjectReader o)
        {
            Texture2D t = new Texture2D(o, _resSData);
            assetTexture2Ds.Add(t);
        }
    }
}
