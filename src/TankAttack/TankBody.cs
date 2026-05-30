using System;
using OpenTK.Graphics.OpenGL;
using Tank.Tools;

namespace Tank
{
    /// <summary>The player's tank hull (textured). Port of CSOpenGL <c>TankBody</c>.</summary>
    public class TankBody : Sprite
    {
        public TankBody(WorldDimensions moveConstrain, TextureCollection textures)
            : base(moveConstrain, textures)
        {
            Position = new float[] { 500, 0, 500 };
            collisionRadius = 8;
            Yaw = 0;
            dimensions = new Dimension3f(5, 7, 8);
        }

        public override void draw()
        {
            textures.useTexture(0);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            GL.Color3(1.0f, 1.0f, 1.0f);

            GL.Begin(PrimitiveType.Quads);

            // top
            GL.TexCoord2(0.52, 0.77); GL.Vertex3(1, 6, 1);
            GL.TexCoord2(0.52, 0.46); GL.Vertex3(4, 6, 1);
            GL.TexCoord2(0.00, 0.46); GL.Vertex3(4, 6, 7);
            GL.TexCoord2(0.00, 0.77); GL.Vertex3(1, 6, 7);

            // back slanted
            GL.TexCoord2(0.89, 0.35); GL.Vertex3(0, 3, 0);
            GL.TexCoord2(0.75, 0.40); GL.Vertex3(1, 6, 1);
            GL.TexCoord2(0.75, 0.72); GL.Vertex3(4, 6, 1);
            GL.TexCoord2(0.89, 0.78); GL.Vertex3(5, 3, 0);

            // back flat
            GL.TexCoord2(1.00, 0.72); GL.Vertex3(1, 0, 0);
            GL.TexCoord2(0.88, 0.72); GL.Vertex3(1, 3, 0);
            GL.TexCoord2(0.88, 0.40); GL.Vertex3(4, 3, 0);
            GL.TexCoord2(1.00, 0.40); GL.Vertex3(4, 0, 0);

            // left side slanted
            GL.TexCoord2(0.56, 0.11); GL.Vertex3(0, 3, 0);
            GL.TexCoord2(0.62, 0.00); GL.Vertex3(1, 6, 1);
            GL.TexCoord2(0.96, 0.00); GL.Vertex3(1, 6, 7);
            GL.TexCoord2(1.00, 0.11); GL.Vertex3(0, 3, 8);

            // front slanted
            GL.TexCoord2(0.64, 0.78); GL.Vertex3(5, 3, 8);
            GL.TexCoord2(0.51, 0.72); GL.Vertex3(4, 6, 7);
            GL.TexCoord2(0.51, 0.40); GL.Vertex3(1, 6, 7);
            GL.TexCoord2(0.64, 0.35); GL.Vertex3(0, 3, 8);

            // front flat
            GL.TexCoord2(0.75, 0.72); GL.Vertex3(4, 0, 8);
            GL.TexCoord2(0.64, 0.72); GL.Vertex3(4, 3, 8);
            GL.TexCoord2(0.64, 0.40); GL.Vertex3(1, 3, 8);
            GL.TexCoord2(0.75, 0.40); GL.Vertex3(1, 0, 8);

            // right side slanted
            GL.TexCoord2(0.56, 0.22); GL.Vertex3(5, 3, 0);
            GL.TexCoord2(0.60, 0.11); GL.Vertex3(4, 6, 1);
            GL.TexCoord2(0.94, 0.11); GL.Vertex3(4, 6, 7);
            GL.TexCoord2(1.00, 0.22); GL.Vertex3(5, 3, 8);

            // tread side left
            GL.TexCoord2(0.77, 1.00); GL.Vertex3(0.2, 0, 0.2);
            GL.TexCoord2(0.77, 0.87); GL.Vertex3(0.2, 3, 0.2);
            GL.TexCoord2(0.00, 0.87); GL.Vertex3(0.2, 3, 7.8);
            GL.TexCoord2(0.00, 1.00); GL.Vertex3(0.2, 0, 7.8);

            // tread side right
            GL.TexCoord2(0.77, 1.00); GL.Vertex3(4.8, 0, 0.2);
            GL.TexCoord2(0.77, 0.87); GL.Vertex3(4.8, 3, 0.2);
            GL.TexCoord2(0.00, 0.87); GL.Vertex3(4.8, 3, 7.8);
            GL.TexCoord2(0.00, 1.00); GL.Vertex3(4.8, 0, 7.8);

            // tread back right
            GL.TexCoord2(0.89, 1.00); GL.Vertex3(0.2, 0, 0.2);
            GL.TexCoord2(0.89, 0.76); GL.Vertex3(0.2, 3, 0.2);
            GL.TexCoord2(1.00, 0.76); GL.Vertex3(1.0, 3, 0.2);
            GL.TexCoord2(1.00, 1.00); GL.Vertex3(1.0, 0, 0.2);

            // tread back left
            GL.TexCoord2(0.89, 1.00); GL.Vertex3(4.0, 0, 0.2);
            GL.TexCoord2(0.89, 0.76); GL.Vertex3(4.0, 3, 0.2);
            GL.TexCoord2(1.00, 0.76); GL.Vertex3(4.8, 3, 0.2);
            GL.TexCoord2(1.00, 1.00); GL.Vertex3(4.8, 0, 0.2);

            GL.End();
            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);
        }

        public override void collisionEffect(Sprite other)
        {
            this.moveBackward();
            if (this.detectCollision(other))
            {
                this.moveForward();
                this.moveForward();
            }
        }
    }
}
