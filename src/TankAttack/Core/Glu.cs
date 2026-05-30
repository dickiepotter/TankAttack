using System;
using OpenTK.Graphics.OpenGL;

namespace Tank.Core
{
    /// <summary>
    /// The GLU helpers Tank Attack used (<c>gluPerspective</c>, <c>gluLookAt</c>).
    /// OpenTK 4 ships no GLU, so they are reproduced here in core GL calls.
    /// </summary>
    public static class Glu
    {
        public static void Perspective(double fovyDegrees, double aspect, double near, double far)
        {
            double top = near * Math.Tan(fovyDegrees * Math.PI / 360.0);
            double right = top * aspect;
            GL.Frustum(-right, right, -top, top, near, far);
        }

        public static void LookAt(
            double eyeX, double eyeY, double eyeZ,
            double centerX, double centerY, double centerZ,
            double upX, double upY, double upZ)
        {
            double fx = centerX - eyeX, fy = centerY - eyeY, fz = centerZ - eyeZ;
            Normalize(ref fx, ref fy, ref fz);

            double sx = fy * upZ - fz * upY;
            double sy = fz * upX - fx * upZ;
            double sz = fx * upY - fy * upX;
            Normalize(ref sx, ref sy, ref sz);

            double ux = sy * fz - sz * fy;
            double uy = sz * fx - sx * fz;
            double uz = sx * fy - sy * fx;

            double[] m =
            {
                sx, ux, -fx, 0,
                sy, uy, -fy, 0,
                sz, uz, -fz, 0,
                0, 0, 0, 1,
            };
            GL.MultMatrix(m);
            GL.Translate(-eyeX, -eyeY, -eyeZ);
        }

        private static void Normalize(ref double x, ref double y, ref double z)
        {
            double len = Math.Sqrt(x * x + y * y + z * z);
            if (len == 0) return;
            x /= len; y /= len; z /= len;
        }
    }
}
