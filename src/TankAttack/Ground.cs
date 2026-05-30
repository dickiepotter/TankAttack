using OpenTK.Graphics.OpenGL;
using Tank.Tools;

namespace Tank
{
    /// <summary>The world's ground, sky lid and surrounding cliff walls — a big
    /// textured box around the play area. Port of CSOpenGL <c>Ground</c>.</summary>
    public class Ground
    {
        private readonly WorldDimensions dimensions;
        private readonly TextureCollection textures;

        public Ground(WorldDimensions dimensions, TextureCollection textures)
        {
            this.textures = textures;
            this.dimensions = dimensions;
        }

        public void draw()
        {
            GL.Color3(1f, 1f, 1f);
            var d = dimensions;

            // Sky (texture 3)
            Quad(3, 0, -1, 0,
                (d.Left, d.Top, d.Near), (d.Left, d.Top, d.Far), (d.Right, d.Top, d.Far), (d.Right, d.Top, d.Near));

            // Ground (texture 1)
            Quad(1, 0, 1, 0,
                (d.Left, d.Bottom, d.Near), (d.Left, d.Bottom, d.Far), (d.Right, d.Bottom, d.Far), (d.Right, d.Bottom, d.Near));

            // Cliff walls (texture 2)
            textures.useTexture(2);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            // front
            GL.Normal3(0, 0, 1);
            GL.TexCoord2(0, 0); GL.Vertex3(d.Left, d.Bottom, d.Near);
            GL.TexCoord2(0, 1); GL.Vertex3(d.Left, d.Top, d.Near);
            GL.TexCoord2(1, 1); GL.Vertex3(d.Right, d.Top, d.Near);
            GL.TexCoord2(1, 0); GL.Vertex3(d.Right, d.Bottom, d.Near);
            // left
            GL.Normal3(1, 0, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(d.Left, d.Bottom, d.Far);
            GL.TexCoord2(0, 1); GL.Vertex3(d.Left, d.Top, d.Far);
            GL.TexCoord2(1, 1); GL.Vertex3(d.Left, d.Top, d.Near);
            GL.TexCoord2(1, 0); GL.Vertex3(d.Left, d.Bottom, d.Near);
            // back
            GL.Normal3(0, 0, -1);
            GL.TexCoord2(0, 0); GL.Vertex3(d.Left, d.Bottom, d.Far);
            GL.TexCoord2(0, 1); GL.Vertex3(d.Left, d.Top, d.Far);
            GL.TexCoord2(1, 1); GL.Vertex3(d.Right, d.Top, d.Far);
            GL.TexCoord2(1, 0); GL.Vertex3(d.Right, d.Bottom, d.Far);
            // right
            GL.Normal3(-1, 0, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(d.Right, d.Bottom, d.Far);
            GL.TexCoord2(0, 1); GL.Vertex3(d.Right, d.Top, d.Far);
            GL.TexCoord2(1, 1); GL.Vertex3(d.Right, d.Top, d.Near);
            GL.TexCoord2(1, 0); GL.Vertex3(d.Right, d.Bottom, d.Near);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        private void Quad(int texture, double nx, double ny, double nz,
            (double x, double y, double z) a, (double x, double y, double z) b,
            (double x, double y, double z) c, (double x, double y, double z) e)
        {
            textures.useTexture(texture);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(nx, ny, nz);
            GL.TexCoord2(0, 0); GL.Vertex3(a.x, a.y, a.z);
            GL.TexCoord2(0, 1); GL.Vertex3(b.x, b.y, b.z);
            GL.TexCoord2(1, 1); GL.Vertex3(c.x, c.y, c.z);
            GL.TexCoord2(1, 0); GL.Vertex3(e.x, e.y, e.z);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }
    }
}
