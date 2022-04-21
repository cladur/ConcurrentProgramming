using Data;
using System;
using System.Collections.ObjectModel;

namespace Logic
{
    internal class Logic : LogicAbstract
    {
        private Random random = new Random();
        private HighResolutionTimer timer;
        DataStorageAbstract dataStorage;
        Vector2 boxSize;
        int startingBalls = 1;

        public Logic(DataStorageAbstract dataStorage, Vector2 boxSize):this(boxSize)
        {
            this.dataStorage = dataStorage;
        }

        public Logic(Vector2 boxSize)
        {
            this.dataStorage = DataStorageAbstract.CreateInstance();
            this.boxSize = boxSize;

            timer = new HighResolutionTimer(8);
            timer.UseHighPriorityThread = false;
            timer.Elapsed += (s, e) => UpdateMovingObjects(8);
            timer.Start();
        }

        public override bool AddBall()
        {
            int maxX = (int)boxSize.X;
            int maxY = (int)boxSize.Y;
            int radius = random.Next(10, 20);
            int x = random.Next(radius, maxX - radius);
            int y = random.Next(radius, maxY - radius);
            Vector2 position = new Vector2(x, y);
            float velX = (float)random.NextDouble() - 0.5f;
            float velY = (float)random.NextDouble() - 0.5f;
            Vector2 velocity = new Vector2(250.0f * velX, 250.0f * velY);
            Ball ball = new Ball(position, velocity, radius);
            dataStorage.Add(ball);
            return true;
        }

        public override bool AddNBalls(int n)
        {
            for (int i = 0; i < n; i++)
            {
                AddBall();
            }
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

        public override bool RemoveMovingObject()
        {
            if (dataStorage.Count() == 0)
                return false;
            dataStorage.RemoveAt(dataStorage.Count() - 1);
            return true;
        }

        private void MoveObject(Data.MovingObject movingObject, Vector2 boxSize, float milliseconds)
        {
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
            movingObject.Move(deltaTime);
        }

        public override void UpdateMovingObjects(float deltaTime)
        {
            for (int i = 0; i < dataStorage.Count(); i++)
            {
                MoveObject(dataStorage.Get(i), boxSize, deltaTime);
            }
        }

        public override ObservableCollection<Data.MovingObject> GetObservableCollection()
        {
            return dataStorage.GetAll();
        }
    }
}
