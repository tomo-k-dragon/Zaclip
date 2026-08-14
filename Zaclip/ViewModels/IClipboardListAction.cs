using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Zaclip.Models;
using Zaclip.Presentations;

namespace Zaclip.ViewModels
{
    public interface IClipboardListAction
    {
        ObservableCollection<ClipboardViewItem> Items { get; }
        bool HasItem { get; }
        ICommand DeleteCommand { get; }
        ICommand SaveCommand { get; }
    }
}
