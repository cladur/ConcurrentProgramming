﻿using Data;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Logic
{
    internal class Logic : LogicAbstract
    {
        private Random random = new Random();
        private HighResolutionTimer timer;
        DataStorageAbstract dataStorage;
        Vector2 boxSize;
        int startingBalls = 1;
        Mutex mutex = new Mutex();
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        Action updateCallback;
        int frames = 8;
        XmlWriterSettings settings;
        XmlWriter writer;

        public Logic(DataStorageAbstract dataStorage, Vector2 boxSize, Action updateCallback) : this(boxSize, updateCallback)
        {
            this.dataStorage = dataStorage;
        }

        public Logic(Vector2 boxSize, Action updateCallback)
        {
            this.dataStorage = DataStorageAbstract.CreateInstance();
            this.boxSize = boxSize;
            this.updateCallback = updateCallback;
            settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            writer = XmlWriter.Create("balls.xml", settings);
            writer.WriteStartElement("root");
            writer.WriteStartElement("balls");

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
            //Console.WriteLine("Created a ball with index: " + dataStorage.Count);
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
            ball.PropertyChanged += BallChanged;
            dataStorage.Add(ball);
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
            Start();
            return true;
        }

        public override async Task<bool> AddNBalls(int n)
        {
            mutex.WaitOne();
            List<Task<bool>> tasks = new List<Task<bool>>();
            for (int i = 0; i < n; i++)
            {
                tasks.Add(AddBall());
            }
            for (int i = 0; i < n; i++)
            {
                await tasks[i];
            }
            mutex.ReleaseMutex();
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
                ball.StartMoving(frames, cancellationToken);
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
            
            // Write ball data to xml
            Ball ball1 = (Ball)ball;
            writer.WriteString(ball1.ToXml());
            // ################
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
                }
            }
        }

        public override void UpdateMovingObjects(float deltaTime)
        {

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
            //movingObject.Move(deltaTime);
        }

        private void ResolveCollision(Data.MovingObject object1, Data.MovingObject object2)
        {
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
