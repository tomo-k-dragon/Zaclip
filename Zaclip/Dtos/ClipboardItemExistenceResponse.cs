using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class ClipboardItemExistenceResponse
    {
        public List<Guid> ExistingGuids { get; set; } = [];
    }
}
