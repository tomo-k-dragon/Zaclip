using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class ClipboardQuery
    {
        public string? Keyword { get; init; }
        public bool? Persisted { get; init; }
        public int Skip { get; init; }
        public int Take { get; init; } = 50;
    }
}
