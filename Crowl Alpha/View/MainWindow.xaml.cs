using CefSharp;
using CefSharp.Wpf;
using Crowl_Alpha.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Crowl_Alpha.View
{
    public partial class MainWindow : Window
    {
        private readonly MainVM MainVM;

        public MainWindow()
        {
            InitializeComponent();

            MainVM = Resources["vm"] as MainVM;

            InitializeBrowser();
        }

        #region ProxySwitch
        private void InitializeBrowser(RequestContext requestContext = null)
        {
            if (browser != null)
            {
                BrowserGrid.Children.Remove(browser);
                browser.Dispose();
            }

            browser = new ChromiumWebBrowser
            {
                RequestContext = requestContext ?? new RequestContext(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Visibility = Visibility.Visible
            };

            SetBrowserBinding();
            BrowserGrid.Children.Add(browser);
            BrowserGrid.UpdateLayout();
        }

        private void SetBrowserBinding()
        {
            BindingOperations.ClearBinding(browser, ChromiumWebBrowser.AddressProperty);
            var binding = new Binding("SearchedUrl") { Source = MainVM };
            BindingOperations.SetBinding(browser, ChromiumWebBrowser.AddressProperty, binding);
            MainVM.Browser= browser;
        }

        public void ApplyProxySettings(bool useProxy)
        {
            var requestContextSettings = new RequestContextSettings();
            var requestContext = new RequestContext(requestContextSettings);

            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                SetProxy(requestContext, useProxy);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        InitializeBrowser(requestContext);
                        Debug.WriteLine("Browser successfully added.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error adding browser: " + ex.Message);
                    }
                });
            });
        }

        private void SetProxy(RequestContext requestContext, bool useProxy)
        {
            var proxyDict = new Dictionary<string, string>
            {
                ["mode"] = useProxy ? "fixed_servers" : "direct"
            };

            if (useProxy)
            {
                proxyDict["server"] = "socks5://127.0.0.1:19050"; // Replace with proxy settings
            }

            requestContext.SetPreference("proxy", proxyDict, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show("Error setting proxy: " + errorMessage);
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainVM.DeactivateTor();
        }

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Find the PackIcon control
            var button = sender as Button;

            // If the context menu exists, open it
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
