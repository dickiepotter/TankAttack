using System;
using System.Collections;
using System.IO;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Tank
{
    /// <summary>
    /// Loads and holds the game's GL textures by index. Port of the CSOpenGL
    /// <c>TextureCollection</c>: the original decoded images with
    /// <c>System.Drawing.Bitmap</c> (Windows-only, and it fed 24-bpp <b>BGR</b>
    /// pixels to GL as <c>GL_RGB</c>, silently swapping red and blue). This uses
    /// StbImageSharp and uploads true RGB, so colours are correct and the loader
    /// is cross-platform. The index-based API is unchanged.
    /// </summary>
    public class TextureCollection : IEnumerable
    {
        private readonly int[] textures;
        private int currentPosition;
        private readonly string texturesDir;

        public TextureCollection(int capacity)
        {
            texturesDir = Path.Combine(AppContext.BaseDirectory, "Textures");
            textures = new int[capacity];
            GL.GenTextures(capacity, textures);
        }

        public int Length => currentPosition;
        public int Capacity => textures.Length;

        public IEnumerator GetEnumerator() => textures.GetEnumerator();

        /// <summary>Load the next texture (kept name <c>loadTextureRGB</c> for parity).</summary>
        public void loadTextureRGB(string file)
        {
            using var stream = File.OpenRead(ResolvePath(file));
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);

            GL.BindTexture(TextureTarget.Texture2D, textures[currentPosition]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
                image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, image.Data);

            currentPosition++;
        }

        public int this[int index] => textures[index];

        public void useTexture(int index) => GL.BindTexture(TextureTarget.Texture2D, textures[index]);

        // The original load list used names like "Tank.bmp" while files on disk are
        // lower-case; resolve case-insensitively so it works on any file system.
        private string ResolvePath(string file)
        {
            string direct = Path.Combine(texturesDir, file);
            if (File.Exists(direct)) return direct;

            foreach (var path in Directory.GetFiles(texturesDir))
                if (string.Equals(Path.GetFileName(path), file, StringComparison.OrdinalIgnoreCase))
                    return path;

            return direct; // let File.OpenRead throw a clear error
        }
    }
}
