using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Data
{
    internal class DataStorage : DataStorageAbstract
    {
        private ObservableCollection<MovingObject> movingObjects = new ObservableCollection<MovingObject>();

        public override void Add(MovingObject movingObject)
        {
            movingObjects.Add(movingObject);
        }

        public override int Count()
        {
            return movingObjects.Count;
        }

        public override MovingObject Get(int index)
        {
            return movingObjects[index];
        }

        public override ObservableCollection<MovingObject> GetAll()
        {
            return new ObservableCollection<Data.MovingObject>(movingObjects);
        }

        public override void Remove(MovingObject movingObject)
        {
            movingObjects.Remove(movingObject);
        }

        public override void RemoveAt(int index)
        {
            movingObjects.RemoveAt(index);
        }
    }
}
