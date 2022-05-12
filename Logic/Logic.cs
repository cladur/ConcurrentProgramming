using Data;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
        Action updateCallback;
        int frames = 4;

        public Logic(DataStorageAbstract dataStorage, Vector2 boxSize, Action updateCallback):this(boxSize, updateCallback)
        {
            this.dataStorage = dataStorage;
        }

        public Logic(Vector2 boxSize, Action updateCallback)
        {
            this.dataStorage = DataStorageAbstract.CreateInstance();
            this.boxSize = boxSize;
            this.updateCallback = updateCallback;

            timer = new HighResolutionTimer(frames);
            timer.UseHighPriorityThread = false;
            timer.Elapsed += (s, e) => UpdateMovingObjects(frames);
            timer.Start();
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
            Ball ball = new Ball(position, velocity, radius, mass);
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
            return true;
        }

        private void MoveObject(Data.MovingObject movingObject, Vector2 boxSize, float milliseconds)
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
            movingObject.Move(deltaTime);
        }

        private void ResolveCollision(Data.MovingObject object1, Data.MovingObject object2)
        {
            Ball b1 = (Ball)object1;
            Ball b2 = (Ball)object2;

            var vel1X = (b1.Mass - b2.Mass) * b1.Velocity.X / (b1.Mass + b2.Mass)
                + (2 * b2.Mass) * b2.Velocity.X  / (b1.Mass + b2.Mass);
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

        private List<Tuple<MovingObject, MovingObject>> DetectCollisions()
        {
            // Collision detection using Sweep and Prune algorithm
            var collisions = new List<Tuple<MovingObject, MovingObject>>();

            if (dataStorage.Count == 0)
            {
                return collisions;
            }

            // 1. Sort balls by X axis
            List<MovingObject> sortedObjects = new List<MovingObject>(dataStorage.GetAll());
            sortedObjects.RemoveAll((MovingObject o) => { return o == null; });
            sortedObjects.Sort((obj1, obj2) => obj1.BoundsLeft.CompareTo(obj2.BoundsLeft));

            float intervalLeft = sortedObjects[0].BoundsLeft;
            float intervalRight = sortedObjects[0].BoundsRight;
            var activeBalls = new List<MovingObject>();
            activeBalls.Add(sortedObjects[0]);
            bool lastUpdated = false;
            for (int i = 1; i < sortedObjects.Count; i++)
            {
                // If ball's interval intersects with one of the active balls interval
                // there's a possibility of collision between them.
                if (intervalRight > sortedObjects[i].BoundsLeft)
                {
                    // Update current intervals
                    intervalRight = Math.Max(intervalRight, sortedObjects[i].BoundsRight);
                    intervalLeft = Math.Min(intervalLeft, sortedObjects[i].BoundsLeft);
                    activeBalls.Add(sortedObjects[i]);
                    lastUpdated = false;
                }
                // In other case, we know that the active balls and the current one are
                // disjointed, so we can ditch the current active ones.
                else
                {
                    // But first we add all possible pairs of collision to the result.
                    for (int j = 0; j < activeBalls.Count; j++)
                    {
                        for (int k = j + 1; k < activeBalls.Count; k++)
                        {
                            collisions.Add(Tuple.Create(activeBalls[j], activeBalls[k]));
                        }
                    }

                    // After that we can clear the active balls, add new ball to it and update intervals.
                    activeBalls.Clear();
                    activeBalls.Add(sortedObjects[i]);
                    intervalLeft = sortedObjects[i].BoundsLeft;
                    intervalRight = sortedObjects[i].BoundsRight;
                    lastUpdated = true;
                }
            }

            if (!lastUpdated)
            {
                // But first we add all possible pairs of collision to the result.
                for (int j = 0; j < activeBalls.Count; j++)
                {
                    for (int k = j + 1; k < activeBalls.Count; k++)
                    {
                        collisions.Add(Tuple.Create(activeBalls[j], activeBalls[k]));
                    }
                }
            }

            return collisions;
        }

        List<List<Tuple<MovingObject, MovingObject>>> IndependentCollisions(List<Tuple<MovingObject, MovingObject>> collisions)
        {
            // Flood-fill algorithm to seperate list of collisions into list
            // of lists of collisions, where inner lists can be processed
            // in parallel.
            // Basically there's a graph, where balls are the nodes and collisions
            // between them are the edges. Our job is to seperate that graph
            // into subgraphs which are disconnected from each other and hence,
            // can be processed in parallel.
            var result = new List<List<Tuple<MovingObject, MovingObject>>>();

            var list = new List<Pair>();

            foreach (var tuple in collisions)
            {
                list.Add(new Pair(tuple, 0));
            }

            int colorInit = 1;
            foreach (var tuple in list)
            {
                var edge = new List<MovingObject>();
                edge.Add(tuple.edge.Item1);
                edge.Add(tuple.edge.Item2);
                if (tuple.color == 0)
                {
                    tuple.color = colorInit++;
                }
                // Go over every single node in graph...
                foreach (var node in edge)
                {
                    // ...and "paint" each edge with it's color
                    foreach (var tuple2 in list)
                    {
                        var edge2 = new List<MovingObject>();
                        edge2.Add(tuple.edge.Item1);
                        edge2.Add(tuple.edge.Item2);
                        foreach (var node2 in edge2)
                        {
                            if (node == node2 && tuple2.color == 0)
                            {
                                tuple2.color = tuple.color;
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < colorInit; i++)
            {
                var disconnectedList = new List<Tuple<MovingObject, MovingObject>>();
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].color == i)
                    {
                        disconnectedList.Add(list[j].edge);
                    }
                }
                result.Add(disconnectedList);
            }
            
            return result;
        }

        public override void UpdateMovingObjects(float deltaTime)
        {
            mutex.WaitOne();
            for (int i = 0; i < dataStorage.Count; i++)
            {
                // move all balls
                MoveObject(dataStorage.Get(i), boxSize, deltaTime);     
            }
            var allCollisions = DetectCollisions();
            var independentCollisions = IndependentCollisions(allCollisions);
            var tasks = new List<Task>();
            foreach (var collisions in independentCollisions)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var collision in collisions)
                    {
                        if (collision.Item1 is Ball ball1 && collision.Item2 is Ball ball2)
                        {
                            if (ball1.isIntersecting(ball2))
                            {
                                ResolveCollision(ball1, ball2);
                            }
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            mutex.ReleaseMutex();
            updateCallback.Invoke();
        }

        public override ObservableCollection<Data.MovingObject> GetObservableCollection()
        {
            return dataStorage.GetAll();
        }
    }

    internal class Pair
    {
        public Tuple<MovingObject, MovingObject> edge;
        public int color;

        public Pair(Tuple<MovingObject, MovingObject> edge, int color)
        {
            this.edge = edge;
            this.color = color;
        }
    }
}
