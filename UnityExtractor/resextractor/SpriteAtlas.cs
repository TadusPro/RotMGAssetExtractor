using System.Drawing;

namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class SpriteAtlas
    {
        private DataReader reader;
        public string Name { get; private set; }
        public int PackedSpritesSize { get; private set; }
        public List<PPtr> PackedSprites { get; private set; }
        public string[] PackedSpriteNamesToIndex { get; private set; }
        public int RenderDataMapSize { get; private set; }
        public List<RenderDataMap> RenderDataMaps { get; private set; }

        public SpriteAtlas(ObjectReader o)
        {
            this.reader = o.Reader;
            reader.SetPosition(o.ByteStart);

            Name = reader.ReadAlignedString();

            PackedSpritesSize = reader.ReadInt();
            PackedSprites = new List<PPtr>(PackedSpritesSize);
            for (int i = 0; i < PackedSpritesSize; i++)
            {
                PackedSprites.Add(new PPtr(reader));
            }

            PackedSpriteNamesToIndex = reader.ReadStringArray();
            RenderDataMapSize = reader.ReadInt();
            RenderDataMaps = new List<RenderDataMap>(RenderDataMapSize);
            for (int i = 0; i < RenderDataMapSize; i++)
            {
                RenderDataMaps.Add(new RenderDataMap(reader));
            }
        }

        public class RenderDataMap
        {
            public byte[] First { get; private set; }
            public long Second { get; private set; }
            public PPtr Texture { get; private set; }
            public PPtr AlphaTexture { get; private set; }
            public Rectangle TextureRect { get; private set; }
            public Vec2f TextureRectOffset { get; private set; }
            public Vec2f AtlasRectOffset { get; private set; }
            public Vec4f UvTransform { get; private set; }
            public float DownscaleMultiplier { get; private set; }
            public long SettingsRaw { get; private set; }
            public long Packed { get; private set; }
            public long PackingMode { get; private set; }
            public long PackingRotation { get; private set; }
            public long MeshType { get; private set; }
            public int SecondaryTexturesSize { get; private set; }
            public List<SecondarySpriteTexture> SecondaryTextures { get; private set; }

            public RenderDataMap(DataReader reader)
            {
                First = reader.ReadBytes(16);
                Second = reader.ReadLong();
                SpriteAtlasData(reader);
            }

            private void SpriteAtlasData(DataReader reader)
            {
                Texture = new PPtr(reader);
                AlphaTexture = new PPtr(reader);
                TextureRect = new Rectangle((int)reader.ReadFloat(), (int)reader.ReadFloat(), (int)reader.ReadFloat(), (int)reader.ReadFloat());
                // BROKE
                TextureRectOffset = new Vec2f(reader.ReadFloat(), reader.ReadFloat());
                AtlasRectOffset = new Vec2f(reader.ReadFloat(), reader.ReadFloat());
                UvTransform = new Vec4f(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                DownscaleMultiplier = reader.ReadFloat();
                SpriteSettings(reader);

                SecondaryTexturesSize = reader.ReadInt();
                SecondaryTextures = new List<SecondarySpriteTexture>(SecondaryTexturesSize);
                for (int i = 0; i < SecondaryTexturesSize; i++)
                {
                    SecondaryTextures.Add(new SecondarySpriteTexture(reader));
                }
                reader.AlignStream();
            }

            private void SpriteSettings(DataReader reader)
            {
                SettingsRaw = reader.ReadUnsignedInt();
                Packed = SettingsRaw & 1;
            }
        }

        public class SecondarySpriteTexture
        {
            public PPtr Texture { get; private set; }
            public string Name { get; private set; }

            public SecondarySpriteTexture(DataReader reader)
            {
                Texture = new PPtr(reader);
                Name = reader.ReadStringToNull();
            }
        }

        public class PPtr
        {
            public int FileId { get; private set; }
            public long PathId { get; private set; }

            public PPtr(DataReader reader)
            {
                FileId = reader.ReadInt();
                PathId = reader.ReadLong();
            }
        }
    }
}