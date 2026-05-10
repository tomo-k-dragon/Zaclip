using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Zaclip.ViewModel;

namespace Zaclip.Command.Common
{
    class WindowHideCommand : ICommand
    {
        private readonly MainViewModel _vm;

        public WindowHideCommand(MainViewModel vm) {
            _vm = vm;
        }

        public bool CanExecute(object? param)
        {
            return true;
        }

        public void Execute(object? param)
        {
            _vm.WindowHide();
        }
        public event EventHandler? CanExecuteChanged;
    }
}
