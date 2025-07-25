using System.Buffers.Binary;

namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class FileHeader
    {
        public const int ResourceFile = 1;
        public const int AssetsFile = 2;

        public long MetadataSize;
        public long FileSize;
        public long Version;
        public long DataOffset;

        public long Size;
        public bool BigEndian;
        public byte[] Reserved;
        public long Unknown;

        public int Type;

        public FileHeader(byte[] data)
        {
            if (data == null)
                throw new ArgumentException($"data is null");

            using (var ms = new MemoryStream(data))
            using (var bs = new BufferedStream(ms, 8))
            using (var reader = new BinaryReader(bs))
            {
                Size = data.Length;
                MetadataSize = ConvertToUnsignedLong(BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)));
                FileSize = ConvertToUnsignedLong(BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)));
                Version = ConvertToUnsignedLong(BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)));
                DataOffset = ConvertToUnsignedLong(BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)));

                if (Version >= 22)
                {
                    BigEndian = reader.ReadBoolean();
                    //Reserved = new byte[3];
                    Reserved = reader.ReadBytes(3);
                    MetadataSize = ConvertToUnsignedLong(BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)));

                    FileSize = BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8));
                    DataOffset = BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8));
                    Unknown = BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8));
                }

                if (Version > 100 || FileSize < 0 || DataOffset < 0 || FileSize > Size || MetadataSize > Size || Version > Size || DataOffset > Size || FileSize < MetadataSize || FileSize < DataOffset)
                {
                    Type = ResourceFile;
                    // log Endian 
                    //Debug.WriteLine("endian is set to " + BigEndian);
                    //Debug.WriteLine("[GameData] Header values are out of expected range. Setting file type as ResourceFile.");
                }
                else
                {
                    //Debug.WriteLine("endian is set to " + BigEndian);

                    Type = AssetsFile;
                    //Debug.WriteLine("[GameData] File type set as AssetsFile based on header values.");
                }
            }
        }


        private long ConvertToUnsignedLong(int value)
        {
            return ((long)value) & 0xFFFFFFFFL;
        }
    }
}