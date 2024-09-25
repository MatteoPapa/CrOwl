using CefSharp;
using CefSharp.Wpf;
using Crowl_Alpha.Model;
using Crowl_Alpha.View;
using Crowl_Alpha.ViewModel.Commands;
using Crowl_Alpha.ViewModel.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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
        public BrowserGoBackCommand BrowserGoBackCommand { get; set; }
        public BrowserGoForwardCommand BrowserGoForwardCommand { get; set; }

        //Relay Commands
        public ICommand ChangeTorNodeCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public object ResourceExtractorTor { get; private set; }

        #endregion

        #region Constructor
        public MainVM()
        {
            TorSwitchCommand = new TorSwitchCommand(this);
            GoToUrlCommand = new GoToUrlCommand(this);
            BrowserGoBackCommand = new BrowserGoBackCommand(this);
            BrowserGoForwardCommand = new BrowserGoForwardCommand(this);

            //Trying RelayCommand

            ChangeTorNodeCommand = new RelayCommand(ExecuteChangeTorNode, CanExecuteChangeTorNode);
            AnalyzeCommand = new RelayCommand(ExecuteAnalyzeCommand, CanExecuteAnalyzeCommand);

            //Subscription to the TorIsReadyEvent
            ResourceExtractorTorHelper.TorReady += TorIsReady;

            //Initializing variables
            SearchIsReadyVariable = true;
            Url = "https://check.torproject.org/";
        }

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
                if (string.IsNullOrEmpty(SearchedUrl) || url != SearchedUrl)
                {
                    if (torEnabled)
                    {
                        Debug.WriteLine($"Visiting {Url} with Tor");
                    }
                    else
                    {
                        Debug.WriteLine($"Visiting {Url} without Tor");
                    }
                    SearchedUrl = Url;
                }
                else
                {
                    Debug.WriteLine("Reloading browser");
                    browser.Reload();
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

        private string hashedPassword = "16:ACFB697EFA9D2E9D60EAEAB60E4F713E7CCBC6E2A8ABE8632DF20446DB";

        private void ActivateTor()
        {
            if (torProcess == null)
            {
                // Generate the content of the torrc file
                string torrcContent = $@"
                    ControlPort 19051
                    HashedControlPassword {hashedPassword}
                    CookieAuthentication 0
                ";

                // Pass the torrc content and additional options (e.g., SocksPort)
                List<string> torArguments = new List<string> {
                    "--SocksPort", "19050"
                };

                torProcess = ResourceExtractorTorHelper.RunEmbeddedExeWithTorrc("tor.exe", torArguments, torrcContent);
            }
            else
            {
                MessageBox.Show("Tor is already initialized", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void DeactivateTor()
        {

            bool wasKilled = ProcessHelper.KillProcess(torProcess);

            if (wasKilled)
            {
                torProcess = null;
                ExecuteToggleProxy();
                Debug.WriteLine("DeactivateTor: Tor process killed successfully. Proxy toggled.");
            }
            else
            {
                Debug.WriteLine("DeactivateTor: Failed to kill the Tor process. Proxy toggle aborted.");
            }
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
            ExecuteToggleProxy();
        }

        private void ExecuteToggleProxy()
        {
            bool useProxy = torEnabled;

            // Access the MainWindow instance on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Safely access MainWindow
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

                // Call the ApplyProxySettings method on the UI thread
                mainWindow.ApplyProxySettings(useProxy);
            });
        }

        private bool _isTimerFinished = true;
        public bool IsTimerFinished
        {
            get => _isTimerFinished;
            set
            {
                if (_isTimerFinished != value)
                {
                    _isTimerFinished = value;
                    OnPropertyChanged(nameof(IsTimerFinished));
                }
            }
        }
        private bool CanExecuteChangeTorNode()
        {
            return IsTimerFinished;
        }
        private async void ExecuteChangeTorNode()
        {
            await TorControlHelper.ChangeTorNode();

            await Task.Delay(100);

            browser.Reload();

            // Disable the MenuItem
            IsTimerFinished = false;

            // Notify that CanExecute may have changed
            (ChangeTorNodeCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Wait for 10 seconds
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Re-enable the MenuItem
            IsTimerFinished = true;

            // Notify that CanExecute may have changed
            (ChangeTorNodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Browser Navigation

        private bool canGoBack;

        public bool CanGoBack
        {
            get => canGoBack;
            set
            {
                if (canGoBack != value)
                {
                    canGoBack = value;
                    OnPropertyChanged("CanGoBack");
                }
            }
        }

        private bool canGoForward;

        public bool CanGoForward
        {
            get => canGoForward;
            set
            {
                if (canGoForward != value)
                {
                    canGoForward = value;
                    OnPropertyChanged("CanGoForward");
                }
            }
        }

        private ChromiumWebBrowser browser;

        public ChromiumWebBrowser Browser
        {
            get => browser;
            set
            {
                if (browser != null)
                {
                    browser.LoadingStateChanged -= OnBrowserLoadingStateChanged;
                    browser.AddressChanged -= OnBrowserAddressChanged;
                }

                if (value != null)
                {
                    browser = value;
                    browser.LoadingStateChanged += OnBrowserLoadingStateChanged;
                    browser.AddressChanged += OnBrowserAddressChanged;
                    OnPropertyChanged("Browser");
                }

            }
        }
        private void OnBrowserLoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            // Ensure this runs on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // These are updated immediately as the navigation state changes
                CanGoForward = e.CanGoForward;
                CanGoBack = e.CanGoBack;

                // Invalidate the command states to refresh the UI
                CommandManager.InvalidateRequerySuggested();
            });
        }
        private void OnBrowserAddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CanGoBack = browser.CanGoBack;
                CanGoForward = browser.CanGoForward;
                // Update searchUrl with the current browser address
                if (browser != null)
                {
                    Url = browser.Address;
                    SearchedUrl = browser.Address; // Assuming searchUrl is a property or field
                }
                CommandManager.InvalidateRequerySuggested();
            });
        }
        public void ExecuteBackCommand()
        {
            if (browser?.CanGoBack == true)
            {
                Browser.Back();
            }

            CanGoBack = browser.CanGoBack;
            CanGoForward = browser.CanGoForward;
        }
        public void ExecuteForwardCommand()
        {
            if (browser?.CanGoForward == true)
            {
                Browser.Forward();
            }
            CanGoBack = browser.CanGoBack;
            CanGoForward = browser.CanGoForward;
        }

        #endregion

        #region Analyze Section

        private ObservableCollection<HtmlNodeInfo> htmlNodes;

        public ObservableCollection<HtmlNodeInfo> HtmlNodes
        {
            get { return htmlNodes; }
            set
            {
                htmlNodes = value;
                OnPropertyChanged("HtmlNodes");
            }
        }


        private bool CanExecuteAnalyzeCommand()
        {
            return true;
        }

        private async void ExecuteAnalyzeCommand()
        {
            try
            {
                // Retrieve the HTML source asynchronously
                string html = await browser.GetSourceAsync();

                // Perform analysis on the HTML
                StartHtmlFragmentation(html);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during analysis
                MessageBox.Show($"Error during analysis: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartHtmlFragmentation(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeInfo rootNodeInfo = HtmlAnalyzerHelper.AnalyzeHtml(doc);

            // Initialize the observable collection and add the root node
            HtmlNodes = new ObservableCollection<HtmlNodeInfo> { rootNodeInfo };

            // Notify that the HtmlNodes collection has changed
            OnPropertyChanged("HtmlNodes");
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
