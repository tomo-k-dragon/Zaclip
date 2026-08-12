using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class ClipboardItemExistenceRequest
    {
        public List<Guid> Guids { get; set; } = [];
    }
}
