namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class SerializedFile
    {
        public long Version;
        public long DataOffset;

        public string UnityVersion;
        public Platform TargetPlatform;
        public bool EnableTypeTree = false;
        public int BigIdEnabled;
        public int ScriptCount;
        public ScriptTypes[] ScriptTypes;
        public int ExternalsCount;
        public Externals[] Externals;
        public int RefTypeCount;
        public SerializedType[] RefTypes;
        public string UserInformation;
        public SerializedType[] Types;
        public long[] TypeDependencies;
        public ObjectReader[] Objects;

        public SerializedFile(byte[] fileData, FileHeader header)
        {
            this.Version = header.Version;
            this.DataOffset = header.DataOffset;

            if (fileData == null)
                throw new ArgumentException($"fileData is null");

            // Create DataReader from the in-memory data.
            DataReader reader = DataReader.GetReader(new MemoryStream(fileData), header.BigEndian);


            for (int i = 0; i < 12; i++)
            {
                reader.ReadInt();  // Assuming these reads are part of setup
            }

            if (Version >= 7)
            {
                UnityVersion = reader.ReadStringToNull();
            }

            if (Version >= 8)
            {
                TargetPlatform = PlatformMethods.ByOrdinal(reader.ReadInt());
            }

            if (Version >= 13)
            {
                EnableTypeTree = reader.ReadBoolean();
            }

            int typeCount = reader.ReadInt();
            Types = SerializedType.GetTypes(this, typeCount, reader);

            BigIdEnabled = 0;
            if (7 <= Version && Version < 14)
            {
                BigIdEnabled = reader.ReadInt();
            }

            int objectCount = reader.ReadInt();
            Objects = new ObjectReader[objectCount];
            for (int i = 0; i < objectCount; i++)
            {
                Objects[i] = new ObjectReader(this, reader);
            }


            if (Version >= 11)
            {
                ScriptCount = reader.ReadInt();
                ScriptTypes = new ScriptTypes[ScriptCount];
                for (int i = 0; i < ScriptCount; i++)
                {
                    ScriptTypes st = new ScriptTypes();
                    st.LocalSerializedFileIndex = reader.ReadInt();
                    if (Version < 14)
                    {
                        st.LocalIdentifierInFile = reader.ReadInt();
                    }
                    else
                    {
                        reader.AlignStream();
                        st.LocalIdentifierInFile = reader.ReadLong();
                    }
                    ScriptTypes[i] = st;
                }
            }

            ExternalsCount = reader.ReadInt();
            Externals = new Externals[ExternalsCount];
            for (int i = 0; i < ExternalsCount; i++)
            {
                Externals ex = new Externals();
                if (Version >= 6)
                {
                    ex.TempEmpty = reader.ReadStringToNull();
                }
                if (Version >= 5)
                {
                    ex.Guid = reader.ReadBytes(16);
                    ex.Type = reader.ReadInt();
                }
                ex.Path = reader.ReadStringToNull();
                Externals[i] = ex;
            }

            if (Version >= 20)
            {
                RefTypeCount = reader.ReadInt();
                RefTypes = SerializedType.GetTypes(this, RefTypeCount, reader);
            }

            if (Version >= 5)
            {
                UserInformation = reader.ReadStringToNull();
            }
        }



    }
    public class ScriptTypes
    {
        public int LocalSerializedFileIndex;
        public long LocalIdentifierInFile;
    }

    public class Externals
    {
        public string TempEmpty;
        public byte[] Guid;
        public int Type;
        public string Path;
    }
}
