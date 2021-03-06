using System;
using Logic;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Presentation.Model
{
    public class Model
    {
        private LogicAbstract logic;

        public Model(float boxWidth, float boxHeight, Action callback)
        {
            logic = LogicAbstract.CreateInstance(boxWidth, boxHeight, callback);
            logic.Start();
        }

        public void AddBall()
        {
            logic.AddBall();
        }

        public void AddNBalls()
        {
            logic.AddNBalls(logic.GetStartingBalls());
        }

        public int GetStartingBalls()
        {
            return logic.GetStartingBalls();
        }

        public void SetStartingBalls(int n)
        {
            logic.SetStartingBalls(n);
        }

        public ObservableCollection<Data.MovingObject> GetMovableObjects()
        {
            return logic.GetObservableCollection();
        }
    }
}
