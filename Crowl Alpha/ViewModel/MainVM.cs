using Crowl_Alpha.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crowl_Alpha.ViewModel
{
    public class MainVM : INotifyPropertyChanged
    {
        #region Props

        private bool torEnabled;

        public bool TorEnabled
        {
            get { return torEnabled; }
            set
            {
                torEnabled = value;
                OnPropertyChanged("TorEnabled");
            }
        }

        #endregion

        #region Commands
        public TorSwitchCommand TorSwitchCommand { get; set; }

        #endregion

        #region Constructor
        public MainVM()
        {
            TorSwitchCommand = new TorSwitchCommand(this);
        }

        #endregion

        #region Methods

        public void TorSwitch()
        {
            if (TorEnabled)
            {
                Debug.WriteLine("True my dear");
            }
            else
            {
                Debug.WriteLine("False my dear");
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
