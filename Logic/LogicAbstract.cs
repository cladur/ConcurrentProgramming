using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Logic
{
    public abstract class LogicAbstract : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract Task<bool> AddBall();

        public abstract Task<bool> AddNBalls(int n);

        public abstract int GetStartingBalls();

        public abstract void SetStartingBalls(int n);


        public abstract void Start();

        public abstract void Stop();

        public static LogicAbstract CreateInstance(float boxWidth, float boxHeight, Action callback)
        {
            return new Logic(new Vector2(boxWidth, boxHeight), callback, LogAbstract.New());
        }
        
        public static LogicAbstract CreateInstance(DataStorageAbstract dataStorage, float boxWidth, float boxHeight, Action updateCallback)
        {
            return new Logic(dataStorage, new Vector2(boxWidth, boxHeight), updateCallback);
        }

        public abstract ObservableCollection<Data.MovingObject> GetObservableCollection();

        protected void OnPropertyChanged(MovingObject ball, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(ball, new PropertyChangedEventArgs(name));
        }
    }
}
