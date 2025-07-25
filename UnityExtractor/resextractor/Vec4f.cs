namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class Vec4f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vec4f(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        // Optionally, you can override ToString() for better debug output
        public override string ToString()
        {
            return $"Vec4f({X}, {Y}, {Z}, {W})";
        }
    }
}