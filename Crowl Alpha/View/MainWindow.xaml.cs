using Crowl_Alpha.ViewModel;
using System.Windows;

namespace Crowl_Alpha.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVM MainVM;
        public MainWindow()
        {
            InitializeComponent();

            MainVM = Resources["vm"] as MainVM;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainVM.DeactivateTor();
        }

    }
}
