using OpenTK.Graphics.OpenGL;
using Tank.Core;
using Tank.Tools;

namespace Tank
{
    /// <summary>The tank's rotating, elevating turret and gun barrel, and the
    /// shell it fires. Port of CSOpenGL <c>Turret</c> (GLU quadric -> Quadric).</summary>
    public class Turret
    {
        private readonly Quadric quadratic;
        private readonly TextureCollection textures;
        private readonly WorldDimensions dimensions;
        private readonly TankBody vehical;
        public Shell shell;

        public Turret(WorldDimensions dimensions, TextureCollection textures, TankBody vehical)
        {
            this.dimensions = dimensions;
            this.textures = textures;
            this.vehical = vehical;
            quadratic = new Quadric(); // gluQuadricNormals(SMOOTH) is the default here
        }

        public void draw()
        {
            textures.useTexture(0);

            GL.PushMatrix();
            GL.Translate(2.5f, 6.2f, 4.2f);
            GL.Rotate(360 - yaw, 0, 1, 0);
            GL.Translate(-2.5f, -6, -4.2f);

            GL.Enable(EnableCap.Texture2D);
            GL.PushMatrix();
            GL.Color3(1.0f, 1.0f, 1.0f);

            GL.Begin(PrimitiveType.Polygon);
            // top
            GL.TexCoord2(0.38, 0.27); GL.Vertex3(2.0, 8, 3.2);
            GL.TexCoord2(0.50, 0.27); GL.Vertex3(3.0, 8, 3.2);
            GL.TexCoord2(0.56, 0.15); GL.Vertex3(3.5, 8, 4.2);
            GL.TexCoord2(0.53, 0.00); GL.Vertex3(3.0, 8, 5.2);
            GL.TexCoord2(0.35, 0.00); GL.Vertex3(2.0, 8, 5.2);
            GL.TexCoord2(0.31, 0.15); GL.Vertex3(1.5, 8, 4.2);
            GL.End();

            GL.Begin(PrimitiveType.Quads);
            // back
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(1.8, 6, 3.0);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(3.2, 6, 3.0);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(3.0, 8, 3.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(2.0, 8, 3.2);
            // left 1
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(3.2, 6, 3);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(3.7, 6, 4);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(3.5, 8, 4.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(3, 8, 3.2);
            // right 1
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(1.3, 6, 4);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(1.8, 6, 3);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(2, 8, 3.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(1.5, 8, 4.2);
            // left 2
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(3.7, 6, 4);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(3.2, 6, 5);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(3, 8, 5.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(3.5, 8, 4.2);
            // right 2
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(1.8, 6, 5);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(1.3, 6, 4);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(1.5, 8, 4.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(2, 8, 5.2);
            // front
            GL.TexCoord2(0.83, 0.34); GL.Vertex3(1.8, 6, 5);
            GL.TexCoord2(1.00, 0.34); GL.Vertex3(3.2, 6, 5);
            GL.TexCoord2(0.98, 0.23); GL.Vertex3(3, 8, 5.2);
            GL.TexCoord2(0.85, 0.23); GL.Vertex3(2, 8, 5.2);
            GL.End();
            GL.PopMatrix();
            GL.Disable(EnableCap.Texture2D);

            // Gun barrel (two cylinders), grey, untextured.
            GL.PushMatrix();
            GL.Color3(0.5f, 0.5f, 0.5f);
            GL.Translate(2.5f, 7.3f, 5);
            GL.Rotate(pitch, 1, 0, 0);
            quadratic.Cylinder(0.4, 0.4, 1, 50, 10);
            quadratic.Cylinder(0.3, 0.3, 5, 50, 10);
            GL.PopMatrix();
            GL.PopMatrix();
        }

        #region Pitch
        private float pitch;
        public void pitchUp()
        {
            if (pitch >= 360) pitch = 0;
            if (pitch < 40 || pitch >= 320) pitch += 1;
        }
        public void pitchDown()
        {
            if (pitch <= 0) pitch = 360;
            if (pitch > 320 || pitch <= 40) pitch -= 1;
        }
        public float Pitch => pitch;
        #endregion

        #region Yaw
        private float yaw;
        public void yawClockwise()
        {
            yaw += 1;
            if (yaw >= 360) yaw = 0;
        }
        public void yawAntiClockwise()
        {
            yaw -= 1;
            if (yaw <= 0) yaw = 360;
        }
        public float Yaw => yaw;
        #endregion

        public void fire()
        {
            if (shell == null || shell.Expired)
                shell = new Shell(dimensions, textures, vehical.Yaw, Yaw, Pitch, vehical.Position);
        }
    }
}
