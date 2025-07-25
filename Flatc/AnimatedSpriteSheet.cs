using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct AnimatedSpriteSheet : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
        public static AnimatedSpriteSheet GetRootAsAnimatedSpriteSheet(ByteBuffer _bb) { return GetRootAsAnimatedSpriteSheet(_bb, new AnimatedSpriteSheet()); }
        public static AnimatedSpriteSheet GetRootAsAnimatedSpriteSheet(ByteBuffer _bb, AnimatedSpriteSheet obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public AnimatedSpriteSheet __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
          public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
        public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
        public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
        public uint Index { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
        public int Set { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
        public uint Direction { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
        public uint Action { get { int o = __p.__offset(12); return o != 0 ? __p.bb.GetUint(o + __p.bb_pos) : (uint)0; } }
        public Sprite? Sprite { get { int o = __p.__offset(14); return o != 0 ? (Sprite?)(new Sprite()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }

        public static Offset<AnimatedSpriteSheet> CreateAnimatedSpriteSheet(FlatBufferBuilder builder,
            StringOffset nameOffset = default(StringOffset),
            uint index = 0,
            int set = 0,
            uint direction = 0,
            uint action = 0,
            Offset<Sprite> spriteOffset = default(Offset<Sprite>))
        {
            builder.StartTable(6);
            AnimatedSpriteSheet.AddSprite(builder, spriteOffset);
            AnimatedSpriteSheet.AddAction(builder, action);
            AnimatedSpriteSheet.AddDirection(builder, direction);
            AnimatedSpriteSheet.AddSet(builder, set);
            AnimatedSpriteSheet.AddIndex(builder, index);
            AnimatedSpriteSheet.AddName(builder, nameOffset);
            return AnimatedSpriteSheet.EndAnimatedSpriteSheet(builder);
        }

        public static void StartAnimatedSpriteSheet(FlatBufferBuilder builder) { builder.StartTable(6); }
        public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
        public static void AddIndex(FlatBufferBuilder builder, uint index) { builder.AddUint(1, index, 0); }
        public static void AddSet(FlatBufferBuilder builder, int set) { builder.AddInt(2, set, 0); }
        public static void AddDirection(FlatBufferBuilder builder, uint direction) { builder.AddUint(3, direction, 0); }
        public static void AddAction(FlatBufferBuilder builder, uint action) { builder.AddUint(4, action, 0); }
        public static void AddSprite(FlatBufferBuilder builder, Offset<Sprite> spriteOffset) { builder.AddOffset(5, spriteOffset.Value, 0); }
        public static Offset<AnimatedSpriteSheet> EndAnimatedSpriteSheet(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<AnimatedSpriteSheet>(o);
        }
    }


    static public class AnimatedSpriteSheetVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyString(tablePos, 4 /*Name*/, false)
              && verifier.VerifyField(tablePos, 6 /*Index*/, 4 /*uint*/, 4, false)
              && verifier.VerifyField(tablePos, 8 /*Set*/, 4 /*int*/, 4, false)
              && verifier.VerifyField(tablePos, 10 /*Direction*/, 4 /*uint*/, 4, false)
              && verifier.VerifyField(tablePos, 12 /*Action*/, 4 /*uint*/, 4, false)
              && verifier.VerifyTable(tablePos, 14 /*Sprite*/, SpriteVerify.Verify, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }
}
