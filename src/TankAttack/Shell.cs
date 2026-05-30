using System;
using OpenTK.Graphics.OpenGL;
using Tank.Core;
using Tank.Tools;

namespace Tank
{
    /// <summary>
    /// The fired shell: a small brass projectile that flies forward until it
    /// expires or hits something. Port of CSOpenGL <c>Shell</c>, reworked from the
    /// original textured fireball sphere into a simple shell shape.
    /// </summary>
    public class Shell : Sprite
    {
        private readonly Quadric quadratic = new Quadric();
        // Shorter flight than the original 400 so a missed shot clears sooner.
        private readonly int moveRange = 200;
        private int currentMoves = 0;
        private float pitch;

        public Shell(WorldDimensions dimensions, TextureCollection textures, float tankYaw, float turretYaw, float initialPitch, float[] initialPosition)
            : base(dimensions, textures)
        {
            this.Yaw = tankYaw - turretYaw; // flight direction
            this.pitch = initialPitch;
            this.dimensions = new Dimension3f(1, 1, 1);
            expired = false;

            // Start at the gun tip: the tank-local gun position (2.5, 7.3, 5)
            // rotated into the world by the tank's yaw, then pushed forward along
            // the firing direction to clear the barrel. The original used a fixed
            // world offset that ignored the tank's facing, so the shell appeared to
            // the side once the tank turned.
            const float gx = 2.5f, gy = 7.3f, gz = 5f;
            double ty = tankYaw * Math.PI / 180;
            float rx = (float)(gx * Math.Cos(ty) + gz * Math.Sin(ty));
            float rz = (float)(-gx * Math.Sin(ty) + gz * Math.Cos(ty));

            double sy = Yaw * Math.PI / 180;
            float fwdX = (float)Math.Sin(sy), fwdZ = (float)Math.Cos(sy);

            this.Position = new float[]
            {
                initialPosition[0] + rx + fwdX * 4f,
                initialPosition[1] + gy,
                initialPosition[2] + rz + fwdZ * 4f,
            };
        }

        public override void draw()
        {
            GL.PushMatrix();
            if (currentMoves < moveRange)
            {
                GL.Translate(this.Position[0], this.Position[1], this.Position[2]);
                GL.Rotate(this.Yaw, 0, 1, 0); // point along travel (+Z local)

                GL.Color3(0.76f, 0.58f, 0.22f); // brass
                quadratic.Cylinder(0.5, 0.5, 2.0, 16, 4); // body
                GL.PushMatrix();
                GL.Translate(0, 0, 2.0);
                quadratic.Cylinder(0.5, 0.0, 1.2, 16, 4); // nose cone
                GL.PopMatrix();

                currentMoves++;
                this.moveForward();
            }
            else
            {
                expired = true;
            }
            GL.PopMatrix();
        }

        // The shell is consumed when it hits something, so you can fire again.
        public override void collisionEffect(Sprite other) => expired = true;

        private bool expired;
        public bool Expired => expired;
    }
}
