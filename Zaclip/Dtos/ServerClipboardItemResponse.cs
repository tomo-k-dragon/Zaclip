using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class ServerClipboardItemResponse
    {
        public Guid Guid { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedAt
        { get; set; }
    }
}
