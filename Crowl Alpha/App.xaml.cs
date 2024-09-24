using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

            var settings = new CefSettings();
            settings.DisableGpuAcceleration(); // Disable DPI scaling  

            // Initialize the CefSharp browser with custom settings  
            Cef.Initialize(settings);
        }
    }
}
