namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class ObjectReader
    {
        public long Version;
        public long DataOffset;
        public DataReader Reader;
        public long PathId;
        public long ByteStartOffset;
        public int BstTuple;
        public long ByteStart;
        public long ByteHeaderOffset;
        public long ByteBaseOffset;
        public long ByteSizeOffset;
        public long ByteSize;
        public int TypeId;
        public SerializedType SerializedType;
        public int ClassId;
        public ClassIDType Type;
        public int IsDestroyed;
        public byte Stripped;
        public short ScriptTypeIndex;

        public ObjectReader(SerializedFile serializedFile, DataReader reader)
        {
            this.Version = serializedFile.Version;
            this.Reader = reader;
            this.DataOffset = serializedFile.DataOffset;

            if (serializedFile.BigIdEnabled != 0)
            {
                this.PathId = Reader.ReadLong();
            }
            else if (Version < 14)
            {
                this.PathId = Reader.ReadInt();
            }
            else
            {
                Reader.AlignStream();
                this.PathId = Reader.ReadLong();
            }

            if (Version >= 22)
            {
                ByteStartOffset = Reader.GetPosition();
                BstTuple = 8;
                ByteStart = Reader.ReadLong();
            }
            else
            {
                ByteStartOffset = Reader.GetPosition();
                BstTuple = 4;
                ByteStart = Reader.ReadInt();
            }

            ByteStart += DataOffset;
            ByteHeaderOffset = DataOffset;
            ByteBaseOffset = 0;

            ByteSizeOffset = Reader.GetPosition();
            ByteSize = Reader.ReadUnsignedInt();

            TypeId = Reader.ReadInt();
            if (Version < 16)
            {
                this.ClassId = Reader.ReadShort();
            }
            else
            {
                var typ = serializedFile.Types[TypeId];
                SerializedType = typ;
                ClassId = typ.ClassId;
            }

            Type = ClassIDTypeHelper.ByOrdinal(ClassId);


            if (Version < 11)
            {
                IsDestroyed = Reader.ReadUnsignedShort();
            }
            else if (Version < 17)
            {
                ScriptTypeIndex = Reader.ReadShort();
            }

            if (Version == 15 || Version == 16)
            {
                Stripped = Reader.ReadByte();
            }
        }

        public override string ToString()
        {
            return $"ObjectReader{{\n   version={Version}\n   data_offset={DataOffset}\n   path_id={PathId}\n   byte_start_offset={ByteStartOffset}\n   bst_tuple={BstTuple}\n   byte_start={ByteStart}\n   byte_header_offset={ByteHeaderOffset}\n   byte_base_offset={ByteBaseOffset}\n   byte_size_offset={ByteSizeOffset}\n   byte_size={ByteSize}\n   type_id={TypeId}\n   serialized_type={SerializedType}\n   class_id={ClassId}\n   type={Type}\n   is_destroyed={IsDestroyed}\n   stripped={Stripped}\n   script_type_index={ScriptTypeIndex}}}";
        }
    }
}