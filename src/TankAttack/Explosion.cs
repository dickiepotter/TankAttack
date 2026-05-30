using OpenTK.Graphics.OpenGL;
using Tank.Tools;

namespace Tank
{
    /// <summary>
    /// A growing, camera-facing fireball shown when a building is hit. Port of the
    /// CSOpenGL <c>Explosion</c>, reworked from a textured cube into a billboard
    /// (a single quad kept facing the camera) with additive blending so the
    /// texture's black background is transparent.
    /// </summary>
    public class Explosion
    {
        private readonly float[] position;
        private readonly TextureCollection textures;
        private int counter = 0;
        private readonly int maxCounter = 200;
        private readonly float maxSize;
        public bool expired = false;

        public Explosion(TextureCollection textures, float yaw, float[] initialPosition, Dimension3f dimensions)
        {
            // Centre the billboard on the building.
            this.position = new float[]
            {
                initialPosition[0] + dimensions.Width / 2,
                initialPosition[1] + dimensions.Height / 2,
                initialPosition[2] + dimensions.Depth / 2,
            };
            this.textures = textures;
            float biggest = dimensions.Width;
            if (dimensions.Height > biggest) biggest = dimensions.Height;
            if (dimensions.Depth > biggest) biggest = dimensions.Depth;
            this.maxSize = biggest; // half-extent the fireball grows to
        }

        public void draw()
        {
            if (counter >= maxCounter) { expired = true; return; }

            float size = (counter / 100f) * maxSize;

            GL.PushMatrix();
            GL.Translate(position[0], position[1], position[2]);

            // Billboard: strip the rotation (and scale) out of the modelview so the
            // quad always faces the camera, keeping only its translation.
            var mv = new float[16];
            GL.GetFloat(GetPName.ModelviewMatrix, mv);
            mv[0] = 1; mv[1] = 0; mv[2] = 0;
            mv[4] = 0; mv[5] = 1; mv[6] = 0;
            mv[8] = 0; mv[9] = 0; mv[10] = 1;
            GL.LoadMatrix(mv);

            GL.Color3(1f, 1f, 1f);
            textures.useTexture(5);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // additive: black -> transparent
            GL.DepthMask(false);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex3(-size, -size, 0);
            GL.TexCoord2(1, 0); GL.Vertex3(size, -size, 0);
            GL.TexCoord2(1, 1); GL.Vertex3(size, size, 0);
            GL.TexCoord2(0, 1); GL.Vertex3(-size, size, 0);
            GL.End();

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            counter++;
        }
    }
}
