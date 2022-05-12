using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Presentation.ViewModel.MVVMLight;
using Presentation.Model;
using System.Windows.Threading;

namespace Presentation.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public Model.Model Model;
        private ObservableCollection<Data.MovingObject> items;

        public ObservableCollection<Data.MovingObject> Items {
            get => items;
            set
            {
                items = value;
                RaisePropertyChanged();
            }
        }

        public int StartingBallCount
        {
            get => Model.GetStartingBalls();
            set => Model.SetStartingBalls(value);
        }
        public ICommand AddBallClick { get; set; }
        public ICommand RemoveBallClick { get; set; }
        public ICommand AddNBallsClick { get; set; }

        public MainViewModel()
        {
            Model = new Model.Model(800, 370, UpdateDisplayedBalls);
            AddBallClick = new RelayCommand(Model.AddBall);
            RemoveBallClick = new RelayCommand(Model.RemoveBall);
            AddNBallsClick = new RelayCommand(Model.AddNBalls);
            Items = new ObservableCollection<Data.MovingObject>();
        }

        public void UpdateDisplayedBalls()
        {
            Items = Model.GetMovableObjects();
        }
    }
}
