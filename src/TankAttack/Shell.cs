using OpenTK.Graphics.OpenGL;
using Tank.Core;
using Tank.Tools;

namespace Tank
{
    /// <summary>The fired shell: a textured sphere that flies forward until it
    /// expires or hits something. Port of CSOpenGL <c>Shell</c>.</summary>
    public class Shell : Sprite
    {
        private readonly Quadric quadratic;
        // Shorter flight than the original 400 so a missed shot clears sooner and
        // you can fire again (only one shell exists at a time).
        private readonly int moveRange = 200;
        private int currentMoves = 0;
        private float pitch;

        public Shell(WorldDimensions dimensions, TextureCollection textures, float initialYaw, float initialPitch, float[] initialPosition)
            : base(dimensions, textures)
        {
            this.Yaw = initialYaw;
            this.pitch = initialPitch;

            // Spawn out the front of the tank along the firing direction (at barrel
            // height) so it appears to leave the gun. The original used a fixed
            // world offset that ignored which way the tank was facing.
            double r = this.Yaw * System.Math.PI / 180;
            float forwardX = (float)System.Math.Sin(r);
            float forwardZ = (float)System.Math.Cos(r);
            this.Position = new float[]
            {
                initialPosition[0] + 2.5f + forwardX * 6f,
                initialPosition[1] + 7.3f,
                initialPosition[2] + 4f + forwardZ * 6f,
            };
            expired = false;

            quadratic = new Quadric { TextureEnabled = true };
            this.dimensions = new Dimension3f(1, 1, 1);
        }

        public override void draw()
        {
            GL.Color3(1f, 1f, 1f);
            textures.useTexture(4);

            GL.PushMatrix();
            if (currentMoves < moveRange)
            {
                GL.Translate(this.Position[0], this.Position[1], this.Position[2]);
                GL.Rotate(this.Yaw, 0, 1, 0);

                GL.Enable(EnableCap.Texture2D);
                // Additive blend so the fireball's black background is transparent.
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                GL.DepthMask(false);
                quadratic.Sphere(dimensions.Width, 50, 10);
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.Texture2D);

                currentMoves++;
                this.moveForward();
            }
            else
            {
                expired = true;
            }
            GL.PopMatrix();
        }

        // The shell is consumed when it hits something, so you can fire again
        // immediately (the original let it fly on through).
        public override void collisionEffect(Sprite other) => expired = true;

        private bool expired;
        public bool Expired => expired;
    }
}
