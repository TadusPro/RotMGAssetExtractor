using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct Position : IFlatbufferObject
    {
        private Struct __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
        public Position __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public float X { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
        public float Y { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }
        public float H { get { return __p.bb.GetFloat(__p.bb_pos + 8); } }
        public float W { get { return __p.bb.GetFloat(__p.bb_pos + 12); } }

        public static Offset<Position> CreatePosition(FlatBufferBuilder builder, float X, float Y, float H, float W)
        {
            builder.Prep(4, 16);
            builder.PutFloat(W);
            builder.PutFloat(H);
            builder.PutFloat(Y);
            builder.PutFloat(X);
            return new Offset<Position>(builder.Offset);
        }
    }
}
