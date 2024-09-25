using CefSharp;
using CefSharp.Wpf;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Crowl_Alpha
{
    /// <summary>  
    /// Interaction logic for App.xaml  
    /// </summary>  
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region Cef Initialize
            var settings = new CefSettings();

            settings.CefCommandLineArgs.Add("disable-back-forward-cache", "1");
            settings.DisableGpuAcceleration(); // Disable DPI scaling  

            // Initialize the CefSharp browser with custom settings  
            Cef.Initialize(settings);
            #endregion

            #region Handling Exceptions
            // Catch UI thread exceptions
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Catch non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            #endregion
        }

        #region Handling Exceptions Methods
        // Handle UI thread exceptions
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Display message box with error details
            MessageBox.Show("An unexpected error occurred:\n" + e.Exception.Message,
                            "Application Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);

            // Set e.Handled to true to prevent application crash
            e.Handled = true;

            // Optionally close the application after the error
            ShutdownApplicationGracefully();
        }

        // Handle non-UI thread exceptions
        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                // Display message box with error details
                MessageBox.Show("A critical error occurred:\n" + ex.Message,
                                "Critical Application Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("A critical error occurred.",
                                "Critical Application Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Close the application gracefully
            ShutdownApplicationGracefully();
        }
        #endregion

        private void ShutdownApplicationGracefully()
        {
            // Optionally do any cleanup here before shutting down the application
            Current.Shutdown();
        }
    }
}
