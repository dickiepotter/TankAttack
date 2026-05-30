namespace Tank.Tools
{
    /// <summary>Bounding extents of the world (bottom/top/left/right/near/far).</summary>
    public class WorldDimensions
    {
        public WorldDimensions(float bottom, float top, float left, float right, float near, float far)
        {
            Bottom = bottom;
            Top = top;
            Left = left;
            Right = right;
            Near = near;
            Far = far;
        }

        public WorldDimensions(float[] dimensions)
            : this(dimensions[0], dimensions[1], dimensions[2], dimensions[3], dimensions[4], dimensions[5]) { }

        public float Bottom { get; }
        public float Top { get; }
        public float Left { get; }
        public float Right { get; }
        public float Near { get; }
        public float Far { get; }

        public float[] ToArray => new[] { Top, Bottom, Left, Right, Near, Far };
    }
}
