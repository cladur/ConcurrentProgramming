using System;
using System.Collections.Generic;
using System.Threading;
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

        public override int Count
        {
            get { return movingObjects.Count; }
        }

        public override MovingObject Get(int index)
        {
            MovingObject movingObject = movingObjects[index];
            return movingObject;
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
