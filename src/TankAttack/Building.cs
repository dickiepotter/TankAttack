using OpenTK.Graphics.OpenGL;
using Tank.Tools;

namespace Tank
{
    /// <summary>
    /// A textured building (cabin / tent / porta-cabin), optionally with a pitched
    /// roof, that explodes when hit by a shell. Port of CSOpenGL <c>Building</c>.
    /// The texture indices passed in select which face uses which loaded texture.
    /// </summary>
    public class Building : Sprite
    {
        private readonly int[] textureIndexs;
        private bool exploding = false;
        public bool expired = false;
        private Explosion explosion = null;

        public Building(WorldDimensions dimensions, TextureCollection textures, Dimension3f buildingSize, int[] indexs)
            : base(dimensions, textures)
        {
            this.dimensions = buildingSize;
            this.textureIndexs = indexs;
            collisionRadius = buildingSize.Width > buildingSize.Depth ? (int)buildingSize.Width : (int)buildingSize.Depth;
        }

        public override void draw()
        {
            if (expired) return;

            GL.Color3(1f, 1f, 1f);

            GL.PushMatrix();
            GL.Translate(this.Position[0], this.Position[1], this.Position[2]);
            GL.Rotate(this.Yaw, 0, 1, 0);

            // Front
            Face(textureIndexs[0], 0, 0, -1,
                (0, dimensions.Height, 0), (0, 0, 0), (dimensions.Width, 0, 0), (dimensions.Width, dimensions.Height, 0));
            // Left
            Face(textureIndexs[1], 1, 0, 0,
                (0, dimensions.Height, dimensions.Depth), (0, 0, dimensions.Depth), (0, 0, 0), (0, dimensions.Height, 0));
            // Back
            Face(textureIndexs[2], 0, 0, 1,
                (dimensions.Width, dimensions.Height, dimensions.Depth), (dimensions.Width, 0, dimensions.Depth),
                (0, 0, dimensions.Depth), (0, dimensions.Height, dimensions.Depth));
            // Right
            Face(textureIndexs[3], -1, 0, 0,
                (dimensions.Width, dimensions.Height, 0), (dimensions.Width, 0, 0),
                (dimensions.Width, 0, dimensions.Depth), (dimensions.Width, dimensions.Height, dimensions.Depth));
            // Flat roof
            Face(textureIndexs[4], -1, 0, 0,
                (0, dimensions.Height, 0), (0, dimensions.Height, dimensions.Depth),
                (dimensions.Width, dimensions.Height, dimensions.Depth), (dimensions.Width, dimensions.Height, 0));

            if (textureIndexs.Length > 5)
            {
                float roofHeight = dimensions.Height + (dimensions.Height / 3);

                // Roof right slope
                Face(textureIndexs[4], -1, 1, 0,
                    (dimensions.Width / 2, roofHeight, dimensions.Depth + 1), (-1, dimensions.Height, dimensions.Depth + 1),
                    (-1, dimensions.Height, -1), (dimensions.Width / 2, roofHeight, -1));
                // Roof left slope
                Face(textureIndexs[5], 1, 1, 0,
                    (dimensions.Width / 2, roofHeight, -1), (dimensions.Width + 1, dimensions.Height, -1),
                    (dimensions.Width + 1, dimensions.Height, dimensions.Depth + 1), (dimensions.Width / 2, roofHeight, dimensions.Depth + 1));
                // Roof front gable
                Triangle(textureIndexs[6], 0, 0, -1,
                    (0, 0), (-1, dimensions.Height, -1),
                    (0.5f, 1), (dimensions.Width / 2, roofHeight, -1),
                    (1, 0), (dimensions.Width + 1, dimensions.Height, -1));
                // Roof back gable
                Triangle(textureIndexs[7], 0, 0, 1,
                    (0, 0), (-1, dimensions.Height, dimensions.Depth + 1),
                    (0.5f, 1), (dimensions.Width / 2, roofHeight, dimensions.Depth + 1),
                    (1, 0), (dimensions.Width + 1, dimensions.Height, dimensions.Depth + 1));
            }
            GL.PopMatrix();

            if (exploding)
            {
                if (explosion.expired)
                {
                    explosion = null;
                    expired = true;
                }
                else
                {
                    explosion.draw();
                }
            }
        }

        // A textured quad: corners are listed with the same (0,0),(0,1),(1,1),(1,0)
        // texture-coordinate winding the original used for every wall.
        private void Face(int texture, double nx, double ny, double nz,
            (double x, double y, double z) a, (double x, double y, double z) b,
            (double x, double y, double z) c, (double x, double y, double z) d)
        {
            textures.useTexture(texture);
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

        private void Triangle(int texture, double nx, double ny, double nz,
            (float s, float t) ta, (double x, double y, double z) a,
            (float s, float t) tb, (double x, double y, double z) b,
            (float s, float t) tc, (double x, double y, double z) c)
        {
            textures.useTexture(texture);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Triangles);
            GL.Normal3(nx, ny, nz);
            GL.TexCoord2(ta.s, ta.t); GL.Vertex3(a.x, a.y, a.z);
            GL.TexCoord2(tb.s, tb.t); GL.Vertex3(b.x, b.y, b.z);
            GL.TexCoord2(tc.s, tc.t); GL.Vertex3(c.x, c.y, c.z);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public override void collisionEffect(Sprite other)
        {
            if (other is Shell)
            {
                exploding = true;
                explosion = new Explosion(textures, Yaw, Position, dimensions);
            }
        }
    }
}
