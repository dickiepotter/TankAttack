namespace Tank.Tools
{
    /// <summary>Width / height / depth of an object.</summary>
    public class Dimension3f
    {
        public Dimension3f(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        public Dimension3f(float[] dimensions)
            : this(dimensions[0], dimensions[1], dimensions[2]) { }

        public float Width { get; }
        public float Height { get; }
        public float Depth { get; }

        public float[] ToArray => new[] { Width, Height, Depth };
    }
}
