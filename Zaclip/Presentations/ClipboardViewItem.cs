using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Models;
using Zaclip.States;

namespace Zaclip.Presentations
{
    public class ClipboardViewItem
    {
        private readonly ClipboardItem _item;
        public int Id => _item.Id;
        public Guid Guid => _item.Guid;
        public string Content => _item.Content;
        public DateTime UpdatedAt => _item.UpdatedAt;
        public bool Persisted => _item.Persisted;

        public SaveDestination SaveState { get; }

        public ClipboardViewItem(
            ClipboardItem item,
            SaveDestination saveState)
        {
            _item = item;
            SaveState = saveState;
        }
    }
}
