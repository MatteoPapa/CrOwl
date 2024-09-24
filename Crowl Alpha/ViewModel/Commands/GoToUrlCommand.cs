using System;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel.Commands
{
    public class GoToUrlCommand : ICommand
    {
        public MainVM VM;

        public GoToUrlCommand(MainVM vm)
        {
            VM = vm;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                bool searchIsReady = (bool)parameter;
                if (searchIsReady)
                {
                    return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }

        }

        public void Execute(object parameter)
        {
            VM.VisitUrl();
        }
    }
}
