using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public abstract class MovingObject
    {
        protected Vector2 position;
        protected Vector2 velocity;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public void Move(float deltaTime) {
            position += velocity * deltaTime;
        }

        public abstract float BoundsRight
        {
            get;
        }

        public abstract float BoundsLeft
        {
            get;
        }

        public abstract float BoundsUp
        {
            get;
        }

        public abstract float BoundsDown
        {
            get;
        }

        public bool IsInside(Vector2 boxSize)
        {
            if (BoundsRight > boxSize.X || BoundsLeft < 0)
            {
                return false;
            }
            if (BoundsUp > boxSize.Y || BoundsDown < 0)
            {
                return false;
            }
            return true;
        }
    }
}
