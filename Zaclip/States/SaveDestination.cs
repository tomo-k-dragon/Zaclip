using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.States
{
    /// <summary>
    /// クリップボードアイテムの保存先を表す列挙型
    /// </summary>
    public enum SaveDestination
    {
        Local,
        Cloud,
        LocalAndCloud,
    }
}
