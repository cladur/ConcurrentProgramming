using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;

namespace Logic
{
    public abstract class LogicAbstract
    {
        public abstract bool AddBall();

        public abstract bool AddNBalls(int n);

        public abstract bool RemoveMovingObject();

        public abstract int GetStartingBalls();

        public abstract void SetStartingBalls(int n);

        public abstract void UpdateMovingObjects(float milliseconds);

        public static LogicAbstract CreateInstance(float boxWidth, float boxHeight)
        {
            return new Logic(new Vector2(boxWidth, boxHeight));
        }

        public static LogicAbstract CreateInstance(DataStorageAbstract dataStorage, float boxWidth, float boxHeight)
        {
            return new Logic(dataStorage, new Vector2(boxWidth, boxHeight));
        }

        public abstract ObservableCollection<Data.MovingObject> GetObservableCollection();
    }
}
