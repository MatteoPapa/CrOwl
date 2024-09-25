using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel.Commands
{
    public class BrowserGoBackCommand : ICommand
    {
        public MainVM VM { get; set; }

        public BrowserGoBackCommand(MainVM vm)
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
            VM.ExecuteBackCommand();
        }
    }
}
