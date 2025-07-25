using System.Diagnostics;
using System.Text;

namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class Texture2D
    {
        private readonly byte[]? _resSData;
        public long PathId { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }

        public bool DownscaleFallback { get; private set; }
        public bool IsAlphaChannelOptional { get; private set; }
        public bool IsReadable { get; private set; }
        public bool IsPreProcessed { get; private set; }
        public bool IgnoreMasterTextureLimit { get; private set; }
        public bool StreamingMipmaps { get; private set; }

        public int ForcedFallbackFormat { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CompleteImageSize { get; private set; }
        public int MipsStripped { get; private set; }
        public int MipCount { get; private set; }
        public int StreamingMipmapsPriority { get; private set; }
        public int ImageCount { get; private set; }
        public int TextureDimension { get; private set; }
        public int FilterMode { get; private set; }
        public int Aniso { get; private set; }
        public int WrapMode { get; private set; }
        public int WrapV { get; private set; }
        public int WrapW { get; private set; }
        public int LightmapFormat { get; private set; }
        public int ColorSpace { get; private set; }
        public int ImageDataSize { get; private set; }

        public float MipBias { get; private set; }

        public long Offset { get; private set; }
        public long Size { get; private set; }
        public byte[] PlatformBlob { get; private set; }
        private byte[] _imageData;

        public byte[] ImageData
        {
            get
            {
                // If already loaded, return cached data.
                if (_imageData != null && _imageData.Length > 0)
                    return _imageData;

                // If embedded but zero-length, nothing to load.
                if (Size == 0 || string.IsNullOrEmpty(Path))
                    return _imageData ?? Array.Empty<byte>();

                // For streamed payload: directly read from the provided .resS data.
                if (_resSData == null)
                {
                    // 'Texture 'DECA' has a streaming path but no .resS data was provided.'
                    Debug.WriteLine($"Texture '{Name}' has a streaming path but no .resS data was provided.");
                    return _imageData ?? Array.Empty<byte>();
                }

                _imageData = new byte[Size];
                Buffer.BlockCopy(_resSData, (int)Offset, _imageData, 0, (int)Size);
                return _imageData;
            }
            private set => _imageData = value; // For embedded data assignment from ctor.
        }


        public TextureFormat TextureFormat { get; private set; }

        public Texture2D(ObjectReader o, byte[]? resSData = null)
        {
            PathId = o.PathId;
            _resSData = resSData;
            var reader = o.Reader;
            reader.SetPosition(o.ByteStart);

            Name = reader.ReadAlignedString();

            ForcedFallbackFormat = reader.ReadInt();
            DownscaleFallback = reader.ReadBoolean();
            IsAlphaChannelOptional = reader.ReadBoolean();
            reader.AlignStream();

            Width = reader.ReadInt();
            Height = reader.ReadInt();
            CompleteImageSize = reader.ReadInt();
            MipsStripped = reader.ReadInt();
            TextureFormat = (TextureFormat)reader.ReadInt();  // Assuming you have an enum corresponding to texture formats
            MipCount = reader.ReadInt();

            IsReadable = reader.ReadBoolean();
            IsPreProcessed = reader.ReadBoolean();
            IgnoreMasterTextureLimit = reader.ReadBoolean();
            StreamingMipmaps = reader.ReadBoolean();
            reader.AlignStream();

            StreamingMipmapsPriority = reader.ReadInt();
            ImageCount = reader.ReadInt();
            TextureDimension = reader.ReadInt();

            GLTextureSettings(reader);

            LightmapFormat = reader.ReadInt();
            ColorSpace = reader.ReadInt();

            PlatformBlob = reader.ReadByteArrayInt();
            reader.AlignStream();

            ImageDataSize = reader.ReadInt();

            if (ImageDataSize > 0)
            {
                ImageData = reader.ReadBytes(ImageDataSize);
            }

            StreamingInfo(reader);
        }

        private void GLTextureSettings(DataReader reader)
        {
            FilterMode = reader.ReadInt();
            Aniso = reader.ReadInt();
            MipBias = reader.ReadFloat();
            WrapMode = reader.ReadInt();
            WrapV = reader.ReadInt();
            WrapW = reader.ReadInt();
        }

        private void StreamingInfo(DataReader reader)
        {
            Offset = reader.ReadLong();
            Size = reader.ReadUnsignedInt();
            Path = reader.ReadAlignedString();
            // if Path is null , it means the texture is not streamed
            // if path is resources.assets.resS like *,resS, it means the texture is streamed

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Texture2D: '{Name}'");
            sb.Append($" ({Width}x{Height}, {TextureFormat})");
            sb.Append($", PathId: {PathId}");

            if (!string.IsNullOrEmpty(Path))
            {
                sb.Append($", Streamed from: {Path} @ {Offset} (Size: {Size})");
            }
            else
            {
                sb.Append($", Embedded Size: {ImageDataSize}");
            }
            sb.Append($", MipCount: {MipCount}");
            sb.Append($", IsReadable: {IsReadable}");
            sb.Append($", IsPreProcessed: {IsPreProcessed}");
            sb.Append($", IgnoreMasterTextureLimit: {IgnoreMasterTextureLimit}");
                
            sb.Append($", StreamingMipmaps: {StreamingMipmaps}");
            sb.Append($", StreamingMipmapsPriority: {StreamingMipmapsPriority}");
            sb.Append($", FilterMode: {FilterMode}");
            sb.Append($", Aniso: {Aniso}");
            sb.Append($", WrapMode: {WrapMode}");
            sb.Append($", WrapV: {WrapV}");
            sb.Append($", WrapW: {WrapW}");
            sb.Append($", LightmapFormat: {LightmapFormat}");
            sb.Append($", ColorSpace: {ColorSpace}");
            sb.Append($", DownscaleFallback: {DownscaleFallback}");
            sb.Append($", IsAlphaChannelOptional: {IsAlphaChannelOptional}");
            sb.Append($", ForcedFallbackFormat: {ForcedFallbackFormat}");
            sb.Append($", CompleteImageSize: {CompleteImageSize}");
            sb.Append($", MipsStripped: {MipsStripped}");
            sb.Append($", ImageCount: {ImageCount}");
            sb.Append($", TextureDimension: {TextureDimension}");
            sb.Append($", MipBias: {MipBias}");
            if (PlatformBlob != null && PlatformBlob.Length > 0)
            {
                sb.Append($", PlatformBlob Size: {PlatformBlob.Length}");
            }
            if (_imageData != null && _imageData.Length > 0)
            {
                sb.Append($", ImageData Size: {_imageData.Length}");
            }
            else
            {
                sb.Append(", ImageData not loaded or empty.");
            }
            if (_resSData != null)
            {
                sb.Append($", .resS data provided, Offset: {Offset}, Size: {Size}");
            }
            else
            {
                sb.Append(", No .resS data provided.");
            }


            return sb.ToString();
        }
    }
}