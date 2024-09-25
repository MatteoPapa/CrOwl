using CefSharp.Wpf;
using System;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel.Commands
{
    public class BrowserGoForwardCommand : ICommand
    {
        public MainVM VM { get; set; }

        public BrowserGoForwardCommand(MainVM vm)
        {
            VM = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.ExecuteForwardCommand();
        }
    }
}
