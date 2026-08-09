using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class ClipboardItemRequest
    {
        public Guid Guid { get; set;  }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
