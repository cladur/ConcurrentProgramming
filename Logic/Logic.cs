using Data;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace Logic
{
    internal class Logic : LogicAbstract
    {
        private Random random = new Random();
        private HighResolutionTimer timer;
        DataStorageAbstract dataStorage;
        private LogAbstract logger;
        Vector2 boxSize;
        int startingBalls = 1;
        Mutex mutex = new Mutex();
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        Action updateCallback;
        int frames = 16;

        public Logic(DataStorageAbstract dataStorage, Vector2 boxSize, Action updateCallback)
        {
            this.boxSize = boxSize;
            this.dataStorage = dataStorage;
            this.updateCallback = updateCallback;
            logger = LogAbstract.New();
        }

        public Logic(Vector2 boxSize, Action updateCallback, LogAbstract logApi)
        {
            this.dataStorage = DataStorageAbstract.CreateInstance();
            this.boxSize = boxSize;
            this.updateCallback = updateCallback;
            this.logger = logApi;

            //timer = new HighResolutionTimer(frames);
            //timer.UseHighPriorityThread = false;
            //timer.Elapsed += (s, e) => UpdateMovingObjects(frames);
            //timer.Start();
        }

        private bool TryAddBall()
        {
            int maxX = (int)boxSize.X;
            int maxY = (int)boxSize.Y;
            int radius = random.Next(15, 25);
            int x = random.Next(radius, maxX - radius);
            int y = random.Next(radius, maxY - radius);
            Vector2 position = new Vector2(x, y);
            float velX = (float)random.NextDouble() - 0.5f;
            float velY = (float)random.NextDouble() - 0.5f;
            Vector2 velocity = new Vector2(250.0f * velX, 250.0f * velY);
            float mass = radius;
            Ball ball = new Ball(dataStorage.Count, position, velocity, radius, mass);
            if (ball == null)
            {
                return false;
            }
            if (dataStorage.Count != 0)
            {
                for (int i = 0; i < dataStorage.Count; i++)
                {
                    if (dataStorage.Get(i) is Ball exisitingBall)
                    {
                        if (ball.isIntersecting(exisitingBall))
                        {
                            return false;
                        }
                    }
                }
            }
            ball.PropertyChanged += BallChanged;
            dataStorage.Add(ball);
            // logging adding ball
            logger.Write("Adding ball", ball);
            
            return true;
        }

        public override async Task<bool> AddBall()
        {
            mutex.WaitOne();
            await Task.Run(() =>
            {
                bool flag = true;
                while (flag)
                {
                    flag = !TryAddBall();
                }
            });
            mutex.ReleaseMutex();
            Stop();
            // todo: it sometmes throws null pointer ex here when initializing the first N balls
            Start();
            return true;
        }

        public override async Task<bool> AddNBalls(int n)
        {
            for (int i = 0; i < n; i++)
            {
                await AddBall();
            }
            /* mutex.WaitOne();
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int i = 0; i < n; i++)
            {
                tasks.Add(AddBall());
            }
            for (int i = 0; i < tasks.Count; i++)
            {
                await tasks[i];
            }
            mutex.ReleaseMutex();*/
            return true;
        }

        public override int GetStartingBalls()
        {
            return startingBalls;
        }

        public override void SetStartingBalls(int n)
        {
            startingBalls = n;
        }

        public override async Task<bool> RemoveMovingObject()
        {
            if (dataStorage.Count == 0)
                return false;
            await Task.Run(() =>
            {
                mutex.WaitOne();
                logger.Write("Removing ball", dataStorage.Get(dataStorage.Count - 1));
                dataStorage.RemoveAt(dataStorage.Count - 1);
                mutex.ReleaseMutex();
            });
            Stop();
            Start();
            return true;
        }

        public override void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            foreach (var ball in dataStorage.GetAll())
            {
                if (ball != null)
                {
                    ball.StartMoving(frames, cancellationToken);
                }
            }
        }

        public override void Stop()
        {
            if (cancellationToken != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }

        void BallChanged(object sender, PropertyChangedEventArgs args)
        {
            MovingObject ball = (MovingObject)sender;
            Update(ball);
        }

        void Update(MovingObject ball)
        {
            mutex.WaitOne();
            if (ball == null)
            {
                mutex.ReleaseMutex();
                return;
            }
            WallCollision(ball, boxSize, frames);
            BallCollisions(dataStorage.GetAll(), ball.Index);
            OnPropertyChanged(ball);
            
            mutex.ReleaseMutex();
            updateCallback.Invoke();
        }

        private void BallCollisions(ObservableCollection<MovingObject> balls, int mainIndex)
        {
            var currentBall = (Ball)balls[mainIndex];
            for (int i = 0; i < balls.Count; i++)
            {
                if (i == mainIndex) continue;

                var otherBall = (Ball)balls[i];
                if (currentBall.isIntersecting(otherBall))
                {
                    ResolveCollision(currentBall, otherBall);
                    logger.Write("Collisions", new List<object> { currentBall, otherBall });
                }
            }
        }

        private void WallCollision(Data.MovingObject movingObject, Vector2 boxSize, float milliseconds)
        {
            if (movingObject is null)
            {
                return;
            }
            // Converting from milliseconds to seconds
            float deltaTime = milliseconds / 1000;
            // Check for wall collision
            if (movingObject.BoundsRight + movingObject.Velocity.X * deltaTime > boxSize.X)
                movingObject.Velocity.X *= -1;
            if (movingObject.BoundsLeft + movingObject.Velocity.X * deltaTime < 0)
                movingObject.Velocity.X *= -1;
            if (movingObject.BoundsUp + movingObject.Velocity.Y * deltaTime > boxSize.Y)
                movingObject.Velocity.Y *= -1;
            if (movingObject.BoundsDown + movingObject.Velocity.Y * deltaTime < 0)
                movingObject.Velocity.Y *= -1;
            logger.Write("Wall collision", movingObject);
            //movingObject.Move(deltaTime);
        }

        private void ResolveCollision(Data.MovingObject object1, Data.MovingObject object2)
        {
            // Calling Start and Stop later in this method solves the glitching balls thing (i think?) but it also makes the program run laggy
            //Stop();
            Ball b1 = (Ball)object1;
            Ball b2 = (Ball)object2;

            var vel1X = (b1.Mass - b2.Mass) * b1.Velocity.X / (b1.Mass + b2.Mass)
                + (2 * b2.Mass) * b2.Velocity.X / (b1.Mass + b2.Mass);
            var vel1Y = (b1.Mass - b2.Mass) * b1.Velocity.Y / (b1.Mass + b2.Mass)
                + (2 * b2.Mass) * b2.Velocity.Y / (b1.Mass + b2.Mass);
            var vel2X = 2f * b1.Mass * b1.Velocity.X / (b1.Mass + b2.Mass)
                + (b2.Mass - b1.Mass) * b2.Velocity.X / (b1.Mass + b2.Mass);
            var vel2Y = 2f * b1.Mass * b1.Velocity.Y / (b1.Mass + b2.Mass)
                + (b2.Mass - b1.Mass) * b2.Velocity.Y / (b1.Mass + b2.Mass);

            b1.Velocity.X = vel1X;
            b1.Velocity.Y = vel1Y;
            b2.Velocity.X = vel2X;
            b2.Velocity.Y = vel2Y;
            //Start();
            // immediately move the balls to avoid them colliding again without actually moving
            b1.Move(frames * 1f / 1000f);
            b2.Move(frames * 1f / 1000f);
        }

        public override ObservableCollection<MovingObject> GetObservableCollection()
        {
            return dataStorage.GetAll();
        }
    }
}
