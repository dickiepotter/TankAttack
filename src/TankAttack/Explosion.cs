using OpenTK.Graphics.OpenGL;
using Tank.Tools;

namespace Tank
{
    /// <summary>A growing textured explosion box shown when a building is hit.
    /// Port of CSOpenGL <c>Explosion</c>.</summary>
    public class Explosion
    {
        private readonly float[] position;
        private readonly TextureCollection textures;
        private int counter = 0;
        private readonly int maxCounter = 200;
        private readonly Dimension3f buildingSize;
        public bool expired = false;
        private readonly float yaw = 0;

        public Explosion(TextureCollection textures, float yaw, float[] initialPosition, Dimension3f dimensions)
        {
            this.position = new float[] { initialPosition[0] + 5f, initialPosition[1] + 5f, initialPosition[2] + 5f };
            this.textures = textures;
            this.buildingSize = dimensions;
            this.yaw = yaw;
        }

        public void draw()
        {
            GL.Color3(1f, 1f, 1f);

            GL.PushMatrix();
            if (counter < maxCounter)
            {
                GL.Translate(this.position[0], this.position[1], this.position[2]);
                GL.Rotate(this.yaw, 0, 1, 0);
                // Original used integer division (counter/100), so the scale was 0
                // until counter hit 100 then jumped to 1 — the explosion barely
                // showed. Use float division so it grows smoothly from 0.
                float scale = counter / 100f;
                GL.Scale(scale, scale, scale);

                // Additive blend so the explosion texture's black background is
                // transparent and the fire glows over the scene.
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                GL.DepthMask(false);

                float w = buildingSize.Width, h = buildingSize.Height, d = buildingSize.Depth;
                Quad(0, 0, -1, (0, h, 0), (0, 0, 0), (w, 0, 0), (w, h, 0));        // front
                Quad(1, 0, 0, (0, h, d), (0, 0, d), (0, 0, 0), (0, h, 0));        // left
                Quad(0, 0, 1, (w, h, d), (w, 0, d), (0, 0, d), (0, h, d));        // back
                Quad(-1, 0, 0, (w, h, 0), (w, 0, 0), (w, 0, d), (w, h, d));       // right

                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);

                counter++;
            }
            else
            {
                expired = true;
            }
            GL.PopMatrix();
        }

        private void Quad(double nx, double ny, double nz,
            (double x, double y, double z) a, (double x, double y, double z) b,
            (double x, double y, double z) c, (double x, double y, double z) d)
        {
            textures.useTexture(5);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(nx, ny, nz);
            GL.TexCoord2(0, 0); GL.Vertex3(a.x, a.y, a.z);
            GL.TexCoord2(0, 1); GL.Vertex3(b.x, b.y, b.z);
            GL.TexCoord2(1, 1); GL.Vertex3(c.x, c.y, c.z);
            GL.TexCoord2(1, 0); GL.Vertex3(d.x, d.y, d.z);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
