using System;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel.Commands
{
    public class ChangeTorNodeCommand : ICommand
    {
        MainVM VM;

        public ChangeTorNodeCommand(MainVM vm)
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
            VM.ChangeTorNode();
        }
    }
}
