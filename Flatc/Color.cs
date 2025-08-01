﻿using Google.FlatBuffers;

namespace RotMGAssetExtractor.Flatc
{
    public struct Color : IFlatbufferObject
    {
        private Struct __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
        public Color __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public float R { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
        public float G { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }
        public float B { get { return __p.bb.GetFloat(__p.bb_pos + 8); } }
        public float A { get { return __p.bb.GetFloat(__p.bb_pos + 12); } }

        public static Offset<Color> CreateColor(FlatBufferBuilder builder, float R, float G, float B, float A)
        {
            builder.Prep(4, 16);
            builder.PutFloat(A);
            builder.PutFloat(B);
            builder.PutFloat(G);
            builder.PutFloat(R);
            return new Offset<Color>(builder.Offset);
        }
    }
}
