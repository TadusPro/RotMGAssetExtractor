using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct Sprite : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
        public static Sprite GetRootAsSprite(ByteBuffer _bb) { return GetRootAsSprite(_bb, new Sprite()); }
        public static Sprite GetRootAsSprite(ByteBuffer _bb, Sprite obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public Sprite __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public Position? Position { get { int o = __p.__offset(4); return o != 0 ? (Position?)(new Position()).__assign(o + __p.bb_pos, __p.bb) : null; } }
        public Position? MaskPosition { get { int o = __p.__offset(6); return o != 0 ? (Position?)(new Position()).__assign(o + __p.bb_pos, __p.bb) : null; } }
        public int Padding { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
        public int Index { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
        public Color? Color { get { int o = __p.__offset(12); return o != 0 ? (Color?)(new Color()).__assign(o + __p.bb_pos, __p.bb) : null; } }
        public bool IsTransparent { get { int o = __p.__offset(14); return o != 0 ? 0 != __p.bb.Get(o + __p.bb_pos) : (bool)false; } }
        public string Name { get { int o = __p.__offset(16); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(16, 1); }
#else
        public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(16); }
#endif
        public byte[] GetNameArray() { return __p.__vector_as_array<byte>(16); }
        public ulong AtlasId { get { int o = __p.__offset(18); return o != 0 ? __p.bb.GetUlong(o + __p.bb_pos) : (ulong)0; } }

        public static void StartSprite(FlatBufferBuilder builder) { builder.StartTable(8); }
        public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddStruct(0, positionOffset.Value, 0); }
        public static void AddMaskPosition(FlatBufferBuilder builder, Offset<Position> maskPositionOffset) { builder.AddStruct(1, maskPositionOffset.Value, 0); }
        public static void AddPadding(FlatBufferBuilder builder, int padding) { builder.AddInt(2, padding, 0); }
        public static void AddIndex(FlatBufferBuilder builder, int index) { builder.AddInt(3, index, 0); }
        public static void AddColor(FlatBufferBuilder builder, Offset<Color> colorOffset) { builder.AddStruct(4, colorOffset.Value, 0); }
        public static void AddIsTransparent(FlatBufferBuilder builder, bool isTransparent) { builder.AddBool(5, isTransparent, false); }
        public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(6, nameOffset.Value, 0); }
        public static void AddAtlasId(FlatBufferBuilder builder, ulong atlasId) { builder.AddUlong(7, atlasId, 0); }
        public static Offset<Sprite> EndSprite(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<Sprite>(o);
        }
    }


    static public class SpriteVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyField(tablePos, 4 /*Position*/, 16 /*Position*/, 4, false)
              && verifier.VerifyField(tablePos, 6 /*MaskPosition*/, 16 /*Position*/, 4, false)
              && verifier.VerifyField(tablePos, 8 /*Padding*/, 4 /*int*/, 4, false)
              && verifier.VerifyField(tablePos, 10 /*Index*/, 4 /*int*/, 4, false)
              && verifier.VerifyField(tablePos, 12 /*Color*/, 16 /*Color*/, 4, false)
              && verifier.VerifyField(tablePos, 14 /*IsTransparent*/, 1 /*bool*/, 1, false)
              && verifier.VerifyString(tablePos, 16 /*Name*/, false)
              && verifier.VerifyField(tablePos, 18 /*AtlasId*/, 8 /*ulong*/, 8, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }
}
