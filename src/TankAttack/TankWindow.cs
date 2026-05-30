using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tank.Core;
using Tank.Tools;

namespace Tank
{
    /// <summary>
    /// The game window and render loop. Replaces the original CSOpenGL pairing of
    /// <c>Main</c> (a WinForms form hosting a <c>GLSurface</c>) and <c>Renderer</c>:
    /// OpenTK's <see cref="GameWindow"/> owns the context, loop and input.
    /// Renders a main chase view plus a small top-down minimap.
    /// </summary>
    public class TankWindow : GameWindow
    {
        private readonly WorldDimensions world = new WorldDimensions(0, 100, 0, 1000, 0, 1000);
        private TextureCollection textures;

        private Control control;
        private TankBody tankBody;
        private Turret turret;
        private Ground ground;
        private List<Building> buildings;

        private bool fog, lowLight, night;

        private readonly float[] eye = { 2.5f, 10f, -7f };
        private readonly float[] lookAt = { 2.5f, 8f, 4f };

        private readonly string capturePath;
        private readonly int captureAfterFrames;
        private int frame;

        public TankWindow(string capturePath = null, int captureAfterFrames = 2)
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                ClientSize = new Vector2i(936, 592),
                Title = "Tank Attack  (WASD drive/turn, ,/. + PgUp/PgDn aim, Space fire, F1 fog F2 light F3 night)",
                Profile = ContextProfile.Compatability, // fixed-function GL needs compatibility
                APIVersion = new Version(3, 3),
            })
        {
            this.capturePath = capturePath;
            this.captureAfterFrames = captureAfterFrames;
            UpdateFrequency = capturePath != null ? 0 : 60;
        }

        #region View-mode toggles (port of Renderer's toggle* methods)
        public void toggleFog()
        {
            if (fog) { GL.Disable(EnableCap.Fog); GL.Disable(EnableCap.Lighting); fog = false; }
            else { GL.Enable(EnableCap.Fog); GL.Enable(EnableCap.Lighting); fog = true; }
        }

        public void toggleLowLight()
        {
            if (lowLight) { GL.Disable(EnableCap.Light0); GL.Disable(EnableCap.Lighting); lowLight = false; }
            else { GL.Enable(EnableCap.Light0); GL.Enable(EnableCap.Lighting); lowLight = true; }
        }

        public void toggleNight()
        {
            // Faithful to the original, which tested lowLight here rather than night.
            if (lowLight) { GL.Disable(EnableCap.Lighting); night = false; }
            else { GL.Enable(EnableCap.Lighting); night = true; }
        }
        #endregion

        protected override void OnLoad()
        {
            base.OnLoad();
            GLInit();
        }

        private void GLInit()
        {
            float[] lightAmbient = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightPosition = { 500.0f, 10.0f, 500.0f, 0.0f };
            float[] lowLightAmbient = { 0.0f, 1.0f, 0.0f, 1.0f };
            float[] lowLightDiffuse = { 0.0f, 1.0f, 0.0f, 1.0f };

            SetProjection();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Glu.LookAt(eye[0], eye[1], eye[2], lookAt[0], lookAt[1], lookAt[2], 0, 1, 0);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.ClearDepth(1.0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, lightDiffuse);
            GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.Diffuse);
            GL.Enable(EnableCap.ColorMaterial);

            GL.Light(LightName.Light1, LightParameter.Ambient, lightAmbient);
            GL.Light(LightName.Light1, LightParameter.Diffuse, lightDiffuse);
            GL.Light(LightName.Light1, LightParameter.Position, lightPosition);
            GL.Enable(EnableCap.Light1);

            GL.Light(LightName.Light0, LightParameter.Ambient, lowLightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lowLightDiffuse);
            GL.Light(LightName.Light0, LightParameter.Position, lightPosition);

            GL.Fog(FogParameter.FogColor, new[] { 1f, 1f, 1f, 1f });
            GL.Fog(FogParameter.FogDensity, 0.035f);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);

            // Textures (order defines the indices used everywhere else).
            textures = new TextureCollection(13);
            textures.loadTextureRGB("tank.bmp");      // 0
            textures.loadTextureRGB("ground.jpg");    // 1
            textures.loadTextureRGB("cliff.jpg");     // 2
            textures.loadTextureRGB("sky.bmp");       // 3
            textures.loadTextureRGB("fireball.jpg");  // 4
            textures.loadTextureRGB("explosion.jpg"); // 5
            textures.loadTextureRGB("toilet.jpg");    // 6
            textures.loadTextureRGB("toilet_f.jpg");  // 7
            textures.loadTextureRGB("tent.jpg");      // 8
            textures.loadTextureRGB("tent_f.jpg");    // 9
            textures.loadTextureRGB("cabin.jpg");     // 10
            textures.loadTextureRGB("cabin_f.jpg");   // 11
            textures.loadTextureRGB("cabin_w.jpg");   // 12

            tankBody = new TankBody(world, textures);
            turret = new Turret(world, textures, tankBody);
            ground = new Ground(world, textures);
            control = new Control(tankBody, turret, this);

            BuildScene();
        }

        private void BuildScene()
        {
            buildings = new List<Building>();

            void Add(Dimension3f size, int[] tex, float[] pos, float yaw)
            {
                var b = new Building(world, textures, size, tex) { Position = pos, Yaw = yaw };
                buildings.Add(b);
            }

            int[] roofedCabin = { 11, 12, 10, 12, 10, 10, 10, 10 };
            int[] cabin = { 11, 12, 10, 12, 10 };
            int[] portaCabin = { 7, 6, 6, 6, 6 };
            int[] tent = { 8, 9, 9, 9, 9 };

            Add(new Dimension3f(15, 20, 15), roofedCabin, new float[] { 250, 0, 200 }, 270);
            Add(new Dimension3f(20, 20, 15), roofedCabin, new float[] { 750, 0, 200 }, 90);
            Add(new Dimension3f(20, 15, 20), cabin, new float[] { 250, 0, 300 }, 270);
            Add(new Dimension3f(25, 10, 20), cabin, new float[] { 750, 0, 300 }, 90);
            Add(new Dimension3f(25, 25, 25), roofedCabin, new float[] { 200, 0, 500 }, 270);
            Add(new Dimension3f(25, 25, 25), roofedCabin, new float[] { 800, 0, 500 }, 0);
            Add(new Dimension3f(10, 10, 10), portaCabin, new float[] { 300, 0, 700 }, 0);
            Add(new Dimension3f(10, 10, 10), portaCabin, new float[] { 700, 0, 700 }, 0);
            Add(new Dimension3f(50, 35, 80), tent, new float[] { 500, 0, 750 }, 270);
        }

        private void SetProjection()
        {
            // The main view is now fullscreen, so use the full window aspect. (The
            // original hard-coded 1, distorting its non-square window.)
            double aspect = ClientSize.Y > 0 ? (double)ClientSize.X / ClientSize.Y : 1.0;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            // Far plane was 400, but the arena walls sit ~500 from the centre and
            // were being clipped; 2000 lets the whole arena render.
            Glu.Perspective(60, aspect, 0.01, 2000);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            SetProjection();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (capturePath == null)
                control.Update(KeyboardState);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            control?.KeyPressed(e.Key);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            Render();
            SwapBuffers();

            if (capturePath != null && ++frame >= captureAfterFrames)
            {
                Capture(capturePath);
                Close();
            }
        }

        private void Render()
        {
            int w = ClientSize.X, h = ClientSize.Y;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ---- Main chase view (fullscreen) ----
            GL.PushMatrix();
            GL.Viewport(0, 0, w, h);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Glu.LookAt(eye[0], eye[1], eye[2], lookAt[0], lookAt[1], lookAt[2], 0, 1, 0);

            tankBody.draw();
            turret.draw();

            GL.Rotate(360 - tankBody.Yaw, 0, 1, 0);
            GL.Translate(-tankBody.Position[0], -tankBody.Position[1], -tankBody.Position[2]);

            ground.draw();

            if (turret.shell != null)
            {
                if (!turret.shell.Expired) turret.shell.draw();
                else turret.shell = null;
            }

            foreach (var building in buildings)
            {
                if (!building.expired)
                {
                    building.draw();
                    tankBody.detectCollision(building);
                    if (turret.shell != null) turret.shell.detectCollision(building);
                }
            }
            GL.PopMatrix();

            DrawMinimap(w, h);
        }

        /// <summary>A circular, top-down minimap overlaid in the bottom-right corner
        /// of the (fullscreen) main view, masked to a circle with the stencil buffer.</summary>
        private void DrawMinimap(int w, int h)
        {
            int d = (int)(h * 0.3); // diameter
            int margin = 16;
            int vx = w - d - margin, vy = margin;
            GL.Viewport(vx, vy, d, d);

            // Clear just this square's stencil to 0, then stamp a filled circle as 1.
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(vx, vy, d, d);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.ScissorTest);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.ColorMask(false, false, false, false);
            GL.Disable(EnableCap.DepthTest);
            DrawNdcCircle(fan: true);
            GL.ColorMask(true, true, true, true);

            // Draw the map only inside the circle.
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            Glu.Perspective(60, 1, 1, 2000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            float tx = tankBody.Position[0], tz = tankBody.Position[2];
            Glu.LookAt(tx, 600, tz, tx, 0, tz, 0, 0, 1); // straight down; +Z is "up". Depth test off -> painter order.

            ground.drawFloor(); // floor only — the sky lid would block the overhead view
            foreach (var building in buildings)
                if (!building.expired) building.draw();

            GL.PushMatrix();
            GL.Translate(tx, 0, tz);
            GL.Rotate(360 - tankBody.Yaw, 0, 1, 0);
            tankBody.draw();
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);

            GL.Disable(EnableCap.StencilTest);

            // Thin border ring around the map.
            GL.Color3(0.85f, 0.85f, 0.85f);
            GL.LineWidth(2);
            DrawNdcCircle(fan: false);

            GL.Enable(EnableCap.DepthTest);
        }

        // Draw a unit circle filling the current viewport, in NDC (no projection).
        private static void DrawNdcCircle(bool fan)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            const int segments = 64;
            if (fan)
            {
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(0f, 0f);
            }
            else
            {
                GL.Begin(PrimitiveType.LineLoop);
            }
            for (int i = 0; i <= segments; i++)
            {
                double a = 2 * System.Math.PI * i / segments;
                GL.Vertex2(System.Math.Cos(a), System.Math.Sin(a));
            }
            GL.End();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        private void Capture(string path)
        {
            int w = ClientSize.X, h = ClientSize.Y;
            var pixels = new byte[w * h * 4];
            GL.ReadBuffer(ReadBufferMode.Front);
            GL.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            var flipped = new byte[pixels.Length];
            int stride = w * 4;
            for (int y = 0; y < h; y++)
                Array.Copy(pixels, y * stride, flipped, (h - 1 - y) * stride, stride);
            for (int i = 3; i < flipped.Length; i += 4) flipped[i] = 255;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            Png.Save(path, w, h, flipped);
            Console.WriteLine($"Captured {w}x{h} -> {path}");
        }
    }
}
