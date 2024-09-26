using CefSharp;
using CefSharp.Wpf;
using Crowl_Alpha.Model;
using Crowl_Alpha.View;
using Crowl_Alpha.ViewModel.Commands;
using Crowl_Alpha.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using static Crowl_Alpha.View.Behaviors.TreeViewSelectedItemBehavior;

namespace Crowl_Alpha.ViewModel
{
    //TODO:     Generic
    /// - Analyze Toolbar Check and Fix
    /// - Freeze the website
    /// - Highlight SelectedItem in Browser
    /// - New Browser on New Ip Address
    /// -  

    //TODO:     AI Stuff
    /// - Scraping Section
    /// - AI: Chat System -> Understand the target element(s)
    /// - AI:? Automatic creation of scraping code
    /// - AI: Run an automated scraper that collects data

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
        public ICommand StopAnalyzeCommand { get; }
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
            StopAnalyzeCommand = new RelayCommand(ExecuteStopAnalyzeCommand, CanExecuteStopAnalyzeCommand);

            //Subscription to the TorIsReadyEvent
            ResourceExtractorTorHelper.TorReady += TorIsReady;

            //Initializing variables
            SearchIsReadyVariable = true;
            Url = "https://check.torproject.org";

            //Two years later... I found this fix, DON'T REMOVE THAT
            SelectedNode = new HtmlNodeInfo();
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
                (AnalyzeCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                    AnalyzeCleanup();
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

        //Props
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
                    AnalyzeCleanup();
                    browser.LoadingStateChanged += OnBrowserLoadingStateChanged;
                    browser.AddressChanged += OnBrowserAddressChanged;
                    browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;
                    OnPropertyChanged("Browser");
                }

            }
        }

        //Methods
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

                AnalyzeCleanup();
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

        #region Main Section

        //Props

        private string analyzedUrl;
        public string AnalyzedUrl
        {
            get { return analyzedUrl; }
            set
            {
                analyzedUrl = value;
                OnPropertyChanged("AnalyzedUrl");
            }
        }

        private HtmlNodeInfo rootNode;
        public HtmlNodeInfo RootNode
        {
            get { return rootNode; }
            set
            {
                rootNode = value;
                OnPropertyChanged("RootNode");
            }
        }

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

        private HtmlNodeInfo _selectedNode;
        public HtmlNodeInfo SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    _selectedNode = value;
                    OnPropertyChanged(nameof(SelectedNode));
                    Debug.WriteLine($"SelectedNode ID: {SelectedNode.DataUid}");
                }
            }
        }

        private string htmlSourceCode;
        public string HtmlSourceCode
        {
            get { return htmlSourceCode; }
            set
            {
                htmlSourceCode = value;
                OnPropertyChanged("HtmlSourceCode");
            }
        }


        //Starting Analysis
        private bool CanExecuteAnalyzeCommand()
        {
            //Check if the url was already analyzed
            if (!string.IsNullOrEmpty(AnalyzedUrl) || RootNode != null || HtmlNodes != null || SearchedUrl == null)
            {
                return false;
            }
            else return true;
        }
        private async void ExecuteAnalyzeCommand()
        {
            //Check if the url was already analyzed
            if (!string.IsNullOrEmpty(AnalyzedUrl) || RootNode != null || HtmlNodes != null)
            {
                return;
            }

            InjectDataUid();
            await Task.Delay(200);

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

            //Setting Props after Analysis

            HtmlNodes = new ObservableCollection<HtmlNodeInfo> { rootNodeInfo };
            RootNode = rootNodeInfo;
            AnalyzedUrl = SearchedUrl;

            string generatedHtml = HtmlHelper.GenerateHtml(RootNode);
            Debug.WriteLine(generatedHtml);
            HtmlSourceCode = generatedHtml;

            //CanExecuteChanged of AnalyzeCommand
            (AnalyzeCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // Notify that the HtmlNodes collection has changed (Do I Really need this?)
            OnPropertyChanged("HtmlNodes");
        }
        private void InjectDataUid()
        {
            var script = @"
                (function() {
                    var uidCounter = 0;

                    function assignUids(element) {
                        if (element.nodeType === Node.ELEMENT_NODE) {
                            if (!element.hasAttribute('data-uid')) {
                                element.setAttribute('data-uid', 'crowl-' + uidCounter++);
                            }
                            var children = element.children;
                            for (var i = 0; i < children.length; i++) {
                                assignUids(children[i]);
                            }
                        }
                    }

                    assignUids(document.body);
                })();
            ";

            browser.ExecuteScriptAsync(script);
        }
        private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            // The message is the data-uid of the clicked element
            var dataUid = e.Message as string;
            Debug.WriteLine("DataUID: " + dataUid);
            // Find the corresponding HtmlNodeInfo
            var htmlNodeInfo = FindHtmlNodeInfoByUid(RootNode, dataUid);

            // Update the UI on the main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (htmlNodeInfo != null)
                {
                    SelectedNode = htmlNodeInfo;
                }
                else
                {
                    MessageBox.Show($"No HtmlNodeInfo found for data-uid: {dataUid}");
                }
            });
        }
        private HtmlNodeInfo FindHtmlNodeInfoByUid(HtmlNodeInfo node, string dataUid)
        {
            if (node.DataUid == dataUid)
                return node;

            foreach (var child in node.Children)
            {
                var result = FindHtmlNodeInfoByUid(child, dataUid);
                if (result != null)
                    return result;
            }

            return null;
        }
        public IsChildOfPredicate HierarchyPredicate => IsChildOf;
        public bool IsChildOf(object nodeA, object nodeB)
        {
            if (nodeA == null || nodeB == null)
                return false;

            if (nodeA == nodeB)
                return true; // A node is considered a child of itself

            var parentNode = nodeB as HtmlNodeInfo;
            if (parentNode != null && parentNode.Children != null)
            {
                foreach (var child in parentNode.Children)
                {
                    // Recursive call to check the entire subtree
                    if (IsChildOf(nodeA, child))
                        return true;
                }
            }
            return false;
        }

        //Stopping Analysis
        private bool CanExecuteStopAnalyzeCommand()
        {
            return true;
        }
        private void ExecuteStopAnalyzeCommand()
        {
            AnalyzeCleanup();
        }
        private void AnalyzeCleanup()
        {
            AnalyzedUrl = null;
            RootNode = null;
            HtmlNodes = null;
            HtmlSourceCode = null;

            //Toolbar
            SelectedTool = null;
            IsClickListenerEnabled = false;

            //CanExecuteChanged of AnalyzeCommand
            (AnalyzeCommand as RelayCommand)?.RaiseCanExecuteChanged();

        }

        #endregion

        #region Toolbar Section

        //Props
        private string _selectedTool;
        public string SelectedTool
        {
            get => _selectedTool;
            set
            {
                Debug.WriteLine("Value Received: " + value);
                if (string.IsNullOrEmpty(value))
                {
                    _selectedTool = value;
                    // Reset cursor when no tool is selected
                    Mouse.OverrideCursor = null;

                    //Cleanup
                    IsClickListenerEnabled = false;
                }
                else
                {
                    if (value != _selectedTool)
                    {
                        // Set the selected tool
                        _selectedTool = value;

                        if (value == "ClickListener")
                        {
                            IsClickListenerEnabled = true;

                            // Set custom cursor based on the selected tool
                            SetCustomCursor();
                        }
                        else
                        {
                            // Reset cursor when no tool is selected
                            Mouse.OverrideCursor = null;
                            IsClickListenerEnabled = false;
                        }
                    }
                }


                OnPropertyChanged(nameof(SelectedTool));
            }
        }

        //Cursor Methods
        private void SetCustomCursor()
        {
            try
            {
                // Use the pack URI to access the embedded resource
                Uri resourceUri = new Uri("pack://application:,,,/View/Images/crosshair.cur");

                // Get the stream for the resource
                StreamResourceInfo resourceStream = Application.GetResourceStream(resourceUri);

                if (resourceStream != null)
                {
                    using (Stream cursorStream = resourceStream.Stream)
                    {
                        Cursor customCursor = new Cursor(cursorStream);
                        Mouse.OverrideCursor = customCursor;
                    }
                }
                else
                {
                    Console.WriteLine("Resource stream is null, resource not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting cursor: " + ex.Message);
            }
        }

        //Click Listener Methods

        private bool isClickListenerEnabled;
        public bool IsClickListenerEnabled
        {
            get => isClickListenerEnabled;
            set
            {
                if (isClickListenerEnabled != value)
                {
                    isClickListenerEnabled = value;
                    OnPropertyChanged(nameof(IsClickListenerEnabled));

                    // Enable or disable the listener depending on the new value
                    if (isClickListenerEnabled)
                    {
                        InjectClickListener();
                    }
                    else
                    {
                        Debug.WriteLine("Deactivate");
                        RemoveClickListener();
                    }
                }
            }
        }
        private void InjectClickListener()
        {
            var script = @"
        var clickListener = function(event) {
            var element = event.target;
            var dataUid = element.getAttribute('data-uid');
            if (dataUid) {
                CefSharp.PostMessage(dataUid);
            }
        };

        document.addEventListener('click', clickListener, true);
    ";

            browser.ExecuteScriptAsync(script);
        }
        public void RemoveClickListener()
        {
            var script = @"
        document.removeEventListener('click', clickListener, true);
    ";

            browser.ExecuteScriptAsync(script);
        }

        #endregion

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
