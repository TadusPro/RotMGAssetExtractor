namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class Vec2f
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2f(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Optionally, you can override ToString() for better debug output
        public override string ToString()
        {
            return $"Vec2f({X}, {Y})";
        }
    }
}