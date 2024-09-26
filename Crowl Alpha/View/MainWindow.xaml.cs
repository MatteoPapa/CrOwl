using CefSharp;
using CefSharp.Wpf;
using Crowl_Alpha.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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

            CustomizeHtmlHighlighting();
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
            MainVM.Browser = browser;
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

        // This method contains the syntax highlighting customization
        private void CustomizeHtmlHighlighting()
        {
            //DISABLE HYPERLINKS OMG I FOUND THAT
            editor.Options = new TextEditorOptions
            {
                EnableHyperlinks = false,
                ShowTabs = true,
                IndentationSize=8
            };

            // Get the built-in HTML highlighting definition
            var htmlHighlighting = HighlightingManager.Instance.GetDefinition("HTML");

            if (htmlHighlighting != null)
            {


                // Customize HTML Tag color (Light Blue)
                var htmlTagColor = htmlHighlighting.GetNamedColor("HtmlTag");
                if (htmlTagColor != null)
                {
                    htmlTagColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#569CD6"));
                }

                // Customize Attributes color (Light Cyan/Blue)
                var attributeNameColor = htmlHighlighting.GetNamedColor("Attributes");
                if (attributeNameColor != null)
                {
                    attributeNameColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#9CDCFE"));
                }

                // Customize Unknown Attributes (like data-uid) (Light Pink)
                var unknownAttributeColor = htmlHighlighting.GetNamedColor("UnknownAttribute");
                if (unknownAttributeColor != null)
                {
                    unknownAttributeColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#D18A68")); // light pink
                }

                // Customize Assignment color (for = sign) (Yellow)
                var assignmentColor = htmlHighlighting.GetNamedColor("Assignment");
                if (assignmentColor != null)
                {
                    assignmentColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#F9F871")); // light yellow
                }

                // Customize String color (Light Orange for attribute values)
                var stringColor = htmlHighlighting.GetNamedColor("String");
                if (stringColor != null)
                {
                    stringColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#CE9178"));
                }

                // Customize Comment color (Greenish Gray)
                var commentColor = htmlHighlighting.GetNamedColor("Comment");
                if (commentColor != null)
                {
                    commentColor.Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString("#6A9955"));
                }

                // Apply the customized syntax highlighting to the editor
                editor.SyntaxHighlighting = htmlHighlighting;
            }
            else
            {
                MessageBox.Show("HTML syntax highlighting definition not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
