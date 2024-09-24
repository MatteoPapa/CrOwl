using System;
using System.Windows.Input;

namespace Crowl_Alpha.ViewModel.Commands
{
    public class TorSwitchCommand : ICommand
    {
        MainVM VM;

        public TorSwitchCommand(MainVM vm)
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
            VM.TorSwitch();
        }
    }
}
