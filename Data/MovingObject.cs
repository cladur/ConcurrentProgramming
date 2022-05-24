using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Data
{
    public abstract class MovingObject : INotifyPropertyChanged
    {
        protected Vector2 position;
        protected Vector2 velocity;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public int Index
        {
            get;
            set;
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

        public abstract void StartMoving(int interval, CancellationToken cancellationToken);
        
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
