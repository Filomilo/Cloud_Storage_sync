using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_desktop.Core;
using Cloud_Storage_desktop.MVVM.ViewModel;

namespace Cloud_Storage_desktop.MVVM.View
{
    class MainViewModel : ObservableObject
    {
        public HomeViewModel HomeVm { get; set; }


        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }

        }
        public MainViewModel()
        {
            HomeVm = new HomeViewModel();
            CurrentView = new HomeView();
        }
    }
}
