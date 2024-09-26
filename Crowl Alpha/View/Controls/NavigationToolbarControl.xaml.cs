using System.Windows;
using System.Windows.Controls;

namespace Crowl_Alpha.View.Controls
{
    /// <summary>
    /// Interaction logic for NavigationToolbarControl.xaml
    /// </summary>
    public partial class NavigationToolbarControl : UserControl
    {
        public NavigationToolbarControl()
        {
            InitializeComponent();
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
