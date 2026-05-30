using System;
using OpenTK.Graphics.OpenGL;

namespace Tank.Core
{
    /// <summary>
    /// Replacement for the GLU quadric helpers the game used — <c>gluCylinder</c>
    /// (the gun barrel) and <c>gluSphere</c> (the shell). OpenTK 4 dropped GLU, so
    /// they are drawn here in immediate mode with outward normals and, optionally,
    /// texture coordinates (matching <c>gluQuadricNormals(SMOOTH)</c> /
    /// <c>gluQuadricTexture</c>). Replaces the <c>GLUquadric</c> handle.
    /// </summary>
    public class Quadric
    {
        /// <summary>Whether to emit texture coordinates (gluQuadricTexture).</summary>
        public bool TextureEnabled { get; set; }

        /// <summary>gluCylinder: from z=0 (radius <paramref name="baseRadius"/>) to
        /// z=height (radius <paramref name="topRadius"/>), around the +Z axis.</summary>
        public void Cylinder(double baseRadius, double topRadius, double height, int slices, int stacks)
        {
            double slope = (baseRadius - topRadius) / height; // z-component of the side normal
            double nLen = Math.Sqrt(1 + slope * slope);

            for (int i = 0; i < stacks; i++)
            {
                double t0 = (double)i / stacks, t1 = (double)(i + 1) / stacks;
                double z0 = height * t0, z1 = height * t1;
                double r0 = baseRadius + (topRadius - baseRadius) * t0;
                double r1 = baseRadius + (topRadius - baseRadius) * t1;

                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= slices; j++)
                {
                    double theta = 2 * Math.PI * j / slices;
                    double cx = Math.Cos(theta), cy = Math.Sin(theta);
                    GL.Normal3(cx / nLen, cy / nLen, slope / nLen);

                    if (TextureEnabled) GL.TexCoord2((double)j / slices, t0);
                    GL.Vertex3(cx * r0, cy * r0, z0);
                    if (TextureEnabled) GL.TexCoord2((double)j / slices, t1);
                    GL.Vertex3(cx * r1, cy * r1, z1);
                }
                GL.End();
            }
        }

        /// <summary>gluSphere: radius <paramref name="radius"/> about the origin,
        /// poles on the Z axis.</summary>
        public void Sphere(double radius, int slices, int stacks)
        {
            for (int i = 0; i < stacks; i++)
            {
                double lat0 = Math.PI * (-0.5 + (double)i / stacks);
                double lat1 = Math.PI * (-0.5 + (double)(i + 1) / stacks);
                double z0 = Math.Sin(lat0), zr0 = Math.Cos(lat0);
                double z1 = Math.Sin(lat1), zr1 = Math.Cos(lat1);

                GL.Begin(PrimitiveType.QuadStrip);
                for (int j = 0; j <= slices; j++)
                {
                    double lng = 2 * Math.PI * j / slices;
                    double x = Math.Cos(lng), y = Math.Sin(lng);

                    GL.Normal3(x * zr0, y * zr0, z0);
                    if (TextureEnabled) GL.TexCoord2((double)j / slices, (double)i / stacks);
                    GL.Vertex3(radius * x * zr0, radius * y * zr0, radius * z0);

                    GL.Normal3(x * zr1, y * zr1, z1);
                    if (TextureEnabled) GL.TexCoord2((double)j / slices, (double)(i + 1) / stacks);
                    GL.Vertex3(radius * x * zr1, radius * y * zr1, radius * z1);
                }
                GL.End();
            }
        }
    }
}
