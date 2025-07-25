using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct SpriteSheetRoot : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_25_2_10(); }
        public static SpriteSheetRoot GetRootAsSpriteSheetRoot(ByteBuffer _bb) { return GetRootAsSpriteSheetRoot(_bb, new SpriteSheetRoot()); }
        public static SpriteSheetRoot GetRootAsSpriteSheetRoot(ByteBuffer _bb, SpriteSheetRoot obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public static bool VerifySpriteSheetRoot(ByteBuffer _bb) { Google.FlatBuffers.Verifier verifier = new Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("", false, SpriteSheetRootVerify.Verify); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public SpriteSheetRoot __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public SpriteSheet? Sprites(int j) { int o = __p.__offset(4); return o != 0 ? (SpriteSheet?)(new SpriteSheet()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
        public int SpritesLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
        public AnimatedSpriteSheet? AnimatedSprites(int j) { int o = __p.__offset(6); return o != 0 ? (AnimatedSpriteSheet?)(new AnimatedSpriteSheet()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
        public int AnimatedSpritesLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }

        public static Offset<SpriteSheetRoot> CreateSpriteSheetRoot(FlatBufferBuilder builder,
            VectorOffset spritesOffset = default(VectorOffset),
            VectorOffset animated_spritesOffset = default(VectorOffset))
        {
            builder.StartTable(2);
            SpriteSheetRoot.AddAnimatedSprites(builder, animated_spritesOffset);
            SpriteSheetRoot.AddSprites(builder, spritesOffset);
            return SpriteSheetRoot.EndSpriteSheetRoot(builder);
        }

        public static void StartSpriteSheetRoot(FlatBufferBuilder builder) { builder.StartTable(2); }
        public static void AddSprites(FlatBufferBuilder builder, VectorOffset spritesOffset) { builder.AddOffset(0, spritesOffset.Value, 0); }
        public static VectorOffset CreateSpritesVector(FlatBufferBuilder builder, Offset<SpriteSheet>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, Offset<SpriteSheet>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<SpriteSheet>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateSpritesVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<SpriteSheet>>(dataPtr, sizeInBytes); return builder.EndVector(); }
        public static void StartSpritesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static void AddAnimatedSprites(FlatBufferBuilder builder, VectorOffset animatedSpritesOffset) { builder.AddOffset(1, animatedSpritesOffset.Value, 0); }
        public static VectorOffset CreateAnimatedSpritesVector(FlatBufferBuilder builder, Offset<AnimatedSpriteSheet>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
        public static VectorOffset CreateAnimatedSpritesVectorBlock(FlatBufferBuilder builder, Offset<AnimatedSpriteSheet>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateAnimatedSpritesVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<AnimatedSpriteSheet>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateAnimatedSpritesVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<AnimatedSpriteSheet>>(dataPtr, sizeInBytes); return builder.EndVector(); }
        public static void StartAnimatedSpritesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static Offset<SpriteSheetRoot> EndSpriteSheetRoot(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<SpriteSheetRoot>(o);
        }
        public static void FinishSpriteSheetRootBuffer(FlatBufferBuilder builder, Offset<SpriteSheetRoot> offset) { builder.Finish(offset.Value); }
        public static void FinishSizePrefixedSpriteSheetRootBuffer(FlatBufferBuilder builder, Offset<SpriteSheetRoot> offset) { builder.FinishSizePrefixed(offset.Value); }
    }


    static public class SpriteSheetRootVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyVectorOfTables(tablePos, 4 /*Sprites*/, SpriteSheetVerify.Verify, false)
              && verifier.VerifyVectorOfTables(tablePos, 6 /*AnimatedSprites*/, AnimatedSpriteSheetVerify.Verify, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }
}
