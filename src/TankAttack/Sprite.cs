using System;
using Tank.Tools;

namespace Tank
{
    /// <summary>
    /// Base class for movable game objects: position, yaw, world-bounds movement
    /// and circle-radius collision. Port of the CSOpenGL <c>Sprite</c>.
    /// </summary>
    public abstract class Sprite
    {
        protected WorldDimensions movementConstraints;
        protected TextureCollection textures;
        protected float collisionRadius = 5;
        private float yaw;
        private float[] translate;
        protected Dimension3f dimensions;

        protected Sprite(WorldDimensions moveConstrain, TextureCollection textures)
        {
            this.movementConstraints = moveConstrain;
            this.textures = textures;
        }

        public abstract void draw();

        #region Collision
        public float CollisionRadius => collisionRadius;

        public bool detectCollision(Sprite other)
        {
            if (calculateDistance(other) < (collisionRadius + other.collisionRadius))
            {
                collisionEffect(other);
                other.collisionEffect(this);
                return true;
            }
            return false;
        }

        public Coord3f calculateCenter() =>
            new Coord3f(dimensions.Width / 2, dimensions.Height / 2, dimensions.Depth / 2);

        protected double calculateDistance(Sprite other)
        {
            double dx = this.Position[0] - other.Position[0];
            double dz = this.Position[2] - other.Position[2];
            return Math.Sqrt(dx * dx + dz * dz);
        }

        public abstract void collisionEffect(Sprite other);
        #endregion

        #region Yaw
        public float Yaw
        {
            get => yaw;
            set
            {
                if (value <= 360 || value >= 0) yaw = value;
                else throw new ArgumentOutOfRangeException(nameof(value), value, "yaw value must be between 0 & 360");
            }
        }

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
        #endregion

        #region Movement
        public float[] Position
        {
            get => translate;
            set
            {
                if (value.Length == 3) translate = value;
                else throw new ArgumentException("position must be an array of three floating point numbers", nameof(value));
            }
        }

        public void moveForward()
        {
            double radianYaw = yaw * (Math.PI / 180);
            translate[2] += (float)(1 * Math.Cos(radianYaw));
            translate[0] += (float)(1 * Math.Sin(radianYaw));
            validateMove();
        }

        public void moveBackward()
        {
            double radianYaw = yaw * (Math.PI / 180);
            translate[2] -= (float)(1 * Math.Cos(radianYaw));
            translate[0] -= (float)(1 * Math.Sin(radianYaw));
            validateMove();
        }

        private void validateMove()
        {
            if (Position[0] < movementConstraints.Left + CollisionRadius) translate[0] = movementConstraints.Left;
            if (Position[0] > movementConstraints.Right - CollisionRadius) translate[0] = movementConstraints.Right;
            if (Position[2] < movementConstraints.Near + CollisionRadius) translate[2] = movementConstraints.Near;
            if (Position[2] > movementConstraints.Far - CollisionRadius) translate[2] = movementConstraints.Far;
        }
        #endregion
    }
}
