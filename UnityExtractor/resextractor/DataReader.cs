using System.Text;

namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class DataReader : IDisposable
    {
        private readonly Stream stream;
        private readonly BinaryReader reader;
        private readonly long length;
        private readonly bool isBigEndian;

        // Constructor now accepts any Stream:
        public DataReader(Stream stream, long length, bool isBigEndian)
        {
            this.stream = stream;
            this.reader = new BinaryReader(stream, Encoding.UTF8);
            this.length = length;
            this.isBigEndian = isBigEndian;
        }

        public static DataReader GetReader(Stream stream, bool isBigEndian)
        {
            return new DataReader(stream, stream.Length, isBigEndian);
        }

        public long ReadLong()
        {
            if (!isBigEndian)
                return BitConverter.ToInt64(reader.ReadBytes(8), 0);

            var bytes = reader.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public int ReadInt()
        {
            if (!isBigEndian)
                return reader.ReadInt32();

            var bytes = reader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public short ReadShort()
        {
            if (isBigEndian)
                return reader.ReadInt16();

            var bytes = reader.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public float ReadFloat()
        {
            if (isBigEndian)
                return BitConverter.ToSingle(reader.ReadBytes(4), 0);

            var bytes = reader.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public long ReadUnsignedInt()
        {
            return (long)ReadInt();
        }

        public int ReadUnsignedShort()
        {
            return ReadShort();
        }

        public void Read(byte[] buffer)
        {
            reader.Read(buffer, 0, buffer.Length);
        }

        public bool ReadBoolean()
        {
            return reader.ReadBoolean();
        }

        public void AlignStream()
        {
            long skip = (4 - stream.Position % 4) % 4;
            if (skip > 0)
            {
                reader.ReadBytes((int)skip);
            }
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public long GetPosition()
        {
            return stream.Position;
        }

        public void SetPosition(long pos)
        {
            stream.Seek(pos, SeekOrigin.Begin);
        }

        public string ReadStringToNull()
        {
            StringBuilder s = new StringBuilder();
            char c;
            while ((c = (char)reader.ReadByte()) != '\0')
            {
                s.Append(c);
            }
            return s.ToString();
        }

        public string ReadAlignedString()
        {
            int length = ReadInt();
            if (length > 0 && length <= GetRemainingBytes())
            {
                var bytes = reader.ReadBytes(length);
                AlignStream();
                return Encoding.UTF8.GetString(bytes);
            }
            return "";
        }

        private long GetRemainingBytes()
        {
            return length - stream.Position;
        }

        public string[] ReadStringArray()
        {
            int num = ReadInt();
            string[] strs = new string[num];
            for (int i = 0; i < num; i++)
            {
                strs[i] = ReadAlignedString();
            }
            return strs;
        }

        public byte[] ReadByteArrayInt()
        {
            byte[] output = new byte[ReadInt()];
            reader.Read(output, 0, output.Length);
            return output;
        }

        public void Dispose()
        {
            reader?.Dispose();
            stream?.Dispose();
        }
    }
}