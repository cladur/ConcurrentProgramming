using System;
using System.Numerics;

namespace Data
{
    public class Ball : MovingObject
    {
        public Ball(Vector2 position, Vector2 velocity, float radius)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
        }

        private float radius;

        public float Radius
        {
            get { return radius; }
            set { if (value > 0) radius = value; }
        }

        public float Diameter
        {
            get { return radius * 2; }
            set { if (value > 0) radius = value * 2; }
        }

        public float X
        {
            get { return position.X - Radius; }
        }
        public float Y
        {
            get { return position.Y - Radius; }
        }

        public override float BoundsLeft
        {
            get { return position.X - Radius; }
        }

        public override float BoundsRight
        {
            get { return position.X + Radius; }
        }

        public override float BoundsUp
        {
            get { return position.Y + Radius; }
        }

        public override float BoundsDown
        {
            get { return position.Y - Radius; }
        }

        public bool isIntercepting(Ball b2)
        {
            return Position.Distance(b2.Position) <= Radius + b2.Radius;
        }
    }
}
