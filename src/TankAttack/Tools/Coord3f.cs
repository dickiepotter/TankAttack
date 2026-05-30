namespace Tank.Tools
{
    /// <summary>A single-precision 3D coordinate.</summary>
    public class Coord3f
    {
        public Coord3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public float[] ToArray => new[] { X, Y, Z };

        public override string ToString() => $"Co-ordinates \n   X: {X} Y: {Y} Z: {Z}\n";
    }
}
