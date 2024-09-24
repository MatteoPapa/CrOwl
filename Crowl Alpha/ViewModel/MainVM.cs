using Crowl_Alpha.ViewModel.Commands;
using Crowl_Alpha.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel
{
    public class MainVM : INotifyPropertyChanged
    {
        #region Generic Props

        //If you need it...
        #endregion

        #region Commands
        public TorSwitchCommand TorSwitchCommand { get; set; }
        public GoToUrlCommand GoToUrlCommand { get; set; }
        public object ResourceExtractorTor { get; private set; }

        #endregion



        #region Url Management

        private string url;

        public string Url
        {
            get { return url; }
            set { url = value; OnPropertyChanged("Url"); }
        }

        private string searchedUrl;

        public string SearchedUrl
        {
            get { return searchedUrl; }
            set
            {
                searchedUrl = value;
                OnPropertyChanged("SearchedUrl");
            }
        }

        public void VisitUrl()
        {
            if (url != null && SearchIsReadyVariable)
            {
                if (torEnabled)
                {
                    Debug.WriteLine($"Visiting {Url} with Tor");
                }
                else
                {
                    Debug.WriteLine($"Visiting {Url} without Tor");
                    SearchedUrl = Url;
                }
            }
        }

        #endregion

        #region TOR Management
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

        private bool searchIsReadyVariable = true;

        public bool SearchIsReadyVariable
        {
            get { return searchIsReadyVariable; }
            set
            {
                searchIsReadyVariable = value;
                OnPropertyChanged("SearchIsReadyVariable");

                CommandManager.InvalidateRequerySuggested(); //Forcing Update
            }
        }

        private Process torProcess;

        public void TorSwitch()
        {
            if (TorEnabled)
            {
                SearchIsReadyVariable = false;
                ActivateTor();
            }
            else
            {
                DeactivateTor();
                SearchIsReadyVariable = true;
            }
        }

        private void ActivateTor()
        {

            if (torProcess == null)
            {
                List<string> torArguments = new List<string> { "--SocksPort", "19050", "--ControlPort", "19051" };
                torProcess = ResourceExtractorTorHelper.RunEmbeddedExe("tor.exe", torArguments);

            }
            else
            {
                MessageBox.Show("Tor is already initialized", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void DeactivateTor()
        {

            ProcessHelper.killProcess(torProcess);
            torProcess = null;
        }

        private void TorIsReady()
        {
            // Check if we're on the UI thread
            if (Application.Current.Dispatcher.CheckAccess())
            {
                // We are on the UI thread, so we can directly update the property
                SearchIsReadyVariable = true;
            }
            else
            {
                // We are NOT on the UI thread, use the dispatcher to update the property
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchIsReadyVariable = true;
                });
            }
        }

        #endregion

        #region Constructor
        public MainVM()
        {
            TorSwitchCommand = new TorSwitchCommand(this);
            GoToUrlCommand = new GoToUrlCommand(this);

            //Subscription to the TorIsReadyEvent
            ResourceExtractorTorHelper.TorReady += TorIsReady;

            //Initializing variables
            SearchIsReadyVariable = true;
            Url = "https://check.torproject.org/";
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
