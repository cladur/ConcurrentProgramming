using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public class Ball : MovingObject
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private Task task;        
        
        public Ball(Vector2 position, Vector2 velocity, float radius, float mass)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
            Mass = mass;
        }

        public Ball(int index, Vector2 position, Vector2 velocity, float radius, float mass)
        {
            Index = index;
            Position = position;
            Velocity = velocity;
            Radius = radius;
            Mass = mass;
        }

        private float radius;
        private float mass;

        public float Radius
        {
            get { return radius; }
            set { if (value > 0) radius = value; }
        }

        public float Mass
        {
            get { return mass; }
            set { if (value > 0) mass = value; }
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

        public bool isIntersecting(Ball b2)
        {
            return Position.Distance(b2.Position) <= Radius + b2.Radius;
        }

        private async Task Run(int interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Reset();
                stopwatch.Start();
                if (!cancellationToken.IsCancellationRequested)
                {
                    Move(interval/1000f);
                    OnPropertyChanged(); 
                }
                stopwatch.Stop();

                await Task.Delay((int)(interval - stopwatch.ElapsedMilliseconds), cancellationToken);
            }
        }

        public override void StartMoving(int interval, CancellationToken cancellationToken)
        {
            task = Task.Run(() => Run(interval, cancellationToken));
        }

        public string ToXml()
        {
            return string.Format("<ball index=\"{0}\" position=\"{1}\" velocity=\"{2}\" radius=\"{3}\" mass=\"{4}\" />",
                Index, Position, Velocity, Radius, Mass);
        }
    }
}
