using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Services.ClipboardEventService
{
    public interface IClipboardEventService
    {
        event Action<int>? ItemSaved;
        void RaiseItemSaved(int itemId);
    }
}
