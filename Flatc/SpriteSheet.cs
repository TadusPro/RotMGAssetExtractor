using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct SpriteSheet : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
        public static SpriteSheet GetRootAsSpriteSheet(ByteBuffer _bb) { return GetRootAsSpriteSheet(_bb, new SpriteSheet()); }
        public static SpriteSheet GetRootAsSpriteSheet(ByteBuffer _bb, SpriteSheet obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public SpriteSheet __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
        public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
        public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
        public ulong AtlasId { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetUlong(o + __p.bb_pos) : (ulong)0; } }
        public Sprite? Sprites(int j) { int o = __p.__offset(8); return o != 0 ? (Sprite?)(new Sprite()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
        public int SpritesLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }

        public static Offset<SpriteSheet> CreateSpriteSheet(FlatBufferBuilder builder,
            StringOffset nameOffset = default(StringOffset),
            ulong atlasId = 0,
            VectorOffset spritesOffset = default(VectorOffset))
        {
            builder.StartTable(3);
            SpriteSheet.AddAtlasId(builder, atlasId);
            SpriteSheet.AddSprites(builder, spritesOffset);
            SpriteSheet.AddName(builder, nameOffset);
            return SpriteSheet.EndSpriteSheet(builder);
        }

        public static void StartSpriteSheet(FlatBufferBuilder builder) { builder.StartTable(3); }
        public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
        public static void AddAtlasId(FlatBufferBuilder builder, ulong atlasId) { builder.AddUlong(1, atlasId, 0); }
        public static void AddSprites(FlatBufferBuilder builder, VectorOffset spritesOffset) { builder.AddOffset(2, spritesOffset.Value, 0); }
        public static VectorOffset CreateSpritesVector(FlatBufferBuilder builder, Offset<Sprite>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, Offset<Sprite>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<Sprite>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<Sprite>>(dataPtr, sizeInBytes); return builder.EndVector(); }
        public static void StartSpritesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static Offset<SpriteSheet> EndSpriteSheet(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<SpriteSheet>(o);
        }
    }


    static public class SpriteSheetVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyString(tablePos, 4 /*Name*/, false)
              && verifier.VerifyField(tablePos, 6 /*AtlasId*/, 8 /*ulong*/, 8, false)
              && verifier.VerifyVectorOfTables(tablePos, 8 /*Sprites*/, SpriteVerify.Verify, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }
}
