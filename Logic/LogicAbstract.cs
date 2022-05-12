using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;

namespace Logic
{
    public abstract class LogicAbstract
    {
        public abstract Task<bool> AddBall();

        public abstract Task<bool> AddNBalls(int n);

        public abstract Task<bool> RemoveMovingObject();

        public abstract int GetStartingBalls();

        public abstract void SetStartingBalls(int n);

        public abstract void UpdateMovingObjects(float milliseconds);

        public static LogicAbstract CreateInstance(float boxWidth, float boxHeight, Action updateCallback)
        {
            return new Logic(new Vector2(boxWidth, boxHeight), updateCallback);
        }

        public static LogicAbstract CreateInstance(DataStorageAbstract dataStorage, float boxWidth, float boxHeight, Action updateCallback)
        {
            return new Logic(dataStorage, new Vector2(boxWidth, boxHeight), updateCallback);
        }

        public abstract ObservableCollection<Data.MovingObject> GetObservableCollection();
    }
}
