using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tank
{
    /// <summary>
    /// Maps input to tank/turret/renderer actions. Port of the CSOpenGL
    /// <c>Control</c>, adapted from WinForms <c>KeyDown</c> events to per-frame
    /// polling: held keys drive continuous movement/aiming, single presses fire
    /// and toggle the view modes.
    /// </summary>
    public class Control
    {
        private readonly TankBody toMove;
        private readonly Turret toAim;
        private readonly TankWindow toView;

        public Control(TankBody toMove, Turret toAim, TankWindow toView)
        {
            this.toMove = toMove;
            this.toAim = toAim;
            this.toView = toView;
        }

        /// <summary>Held-key actions, called once per frame.</summary>
        public void Update(KeyboardState keyboard)
        {
            // Standard WASD: the original used W/Z to move and A/S to turn (no D),
            // which felt mismapped. W/S now drive, A/D turn the tank.
            if (keyboard.IsKeyDown(Keys.W)) toMove.moveForward();
            if (keyboard.IsKeyDown(Keys.S)) toMove.moveBackward();
            if (keyboard.IsKeyDown(Keys.A)) toMove.yawClockwise();      // turn left
            if (keyboard.IsKeyDown(Keys.D)) toMove.yawAntiClockwise();  // turn right

            if (keyboard.IsKeyDown(Keys.PageUp)) toAim.pitchUp();
            if (keyboard.IsKeyDown(Keys.PageDown)) toAim.pitchDown();
            if (keyboard.IsKeyDown(Keys.Comma)) toAim.yawAntiClockwise();
            if (keyboard.IsKeyDown(Keys.Period)) toAim.yawClockwise();
        }

        /// <summary>Single-press actions.</summary>
        public void KeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.Space: toAim.fire(); break;
                case Keys.F1: toView.toggleFog(); break;
                case Keys.F2: toView.toggleLowLight(); break;
                case Keys.F3: toView.toggleNight(); break;
            }
        }
    }
}
