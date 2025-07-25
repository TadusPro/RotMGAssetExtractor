namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class SerializedType
    {
        public int ClassId;
        public bool IsStrippedType;
        public short ScriptTypeIndex;
        public byte[] ScriptId = new byte[16];
        public byte[] OldTypeHash = new byte[16];

        public static SerializedType[] GetTypes(SerializedFile serializedFile, int typeCount, DataReader reader)
        {
            SerializedType[] types = new SerializedType[typeCount];
            for (int i = 0; i < typeCount; i++)
            {
                var st = new SerializedType();

                st.ClassId = reader.ReadInt();
                if (serializedFile.Version >= 16)
                {
                    st.IsStrippedType = reader.ReadBoolean();
                }
                if (serializedFile.Version >= 17)
                {
                    st.ScriptTypeIndex = reader.ReadShort();
                }
                if (serializedFile.Version >= 13)
                {
                    if ((serializedFile.Version < 16 && st.ClassId < 0) || (serializedFile.Version >= 16 && st.ClassId == 114))
                    {
                        reader.Read(st.ScriptId);
                    }
                    reader.Read(st.OldTypeHash);
                }

                if (serializedFile.EnableTypeTree)
                {
                    if (serializedFile.Version >= 12 || serializedFile.Version == 10)
                    {
                        ReadTypeTreeBlob(reader);
                    }
                    else
                    {
                        ReadTypeTree(reader);
                    }

                    if (serializedFile.Version >= 21)
                    {
                        int typeDepSize = reader.ReadInt();
                        serializedFile.TypeDependencies = new long[typeDepSize];
                        for (int j = 0; j < typeDepSize; j++)
                        {
                            serializedFile.TypeDependencies[j] = reader.ReadUnsignedInt();
                        }
                    }
                    throw new InvalidOperationException("Type tree reading not fully implemented.");
                }
                types[i] = st;
            }
            return types;
        }

        private static void ReadTypeTreeBlob(DataReader reader)
        {
            // Placeholder: Logic to parse the type tree blob should be implemented here.
            throw new NotImplementedException("Type tree blob reading not implemented.");
        }

        private static void ReadTypeTree(DataReader reader)
        {
            // Placeholder: Logic to parse the type tree should be implemented here.
            throw new NotImplementedException("Type tree structure reading not implemented.");
        }
    }
}