using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Data
{
    public abstract class DataStorageAbstract
    {
        public abstract void Add(MovingObject movingObject);
        public abstract int Count
        {
            get;
        }
        public abstract MovingObject Get(int index);
        public abstract void Remove(MovingObject movingObject);
        public abstract void RemoveAt(int index);
        public abstract ObservableCollection<MovingObject> GetAll();

        public static DataStorageAbstract CreateInstance()
        {
            return new DataStorage();
        }
    }
}
