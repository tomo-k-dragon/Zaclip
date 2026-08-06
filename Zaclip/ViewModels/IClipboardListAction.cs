using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Zaclip.Models;

namespace Zaclip.ViewModels
{
    public interface IClipboardListAction
    {
        ObservableCollection<ClipboardItem> Items { get; }
        bool HasItem { get; }
        ICommand DeleteCommand { get; }
        ICommand SaveCommand { get; }
    }
}
