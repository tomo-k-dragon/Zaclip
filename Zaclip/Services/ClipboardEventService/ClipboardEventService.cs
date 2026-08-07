using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Services.ClipboardEventService
{
    public class ClipboardEventService : IClipboardEventService
    {
        public event Action<int>? ItemSaved;
        public void RaiseItemSaved(int itemId)
        {
            ItemSaved?.Invoke(itemId);
        }
    }
}
