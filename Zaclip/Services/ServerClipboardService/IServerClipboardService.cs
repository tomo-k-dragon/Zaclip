using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.ClipboardItemsService
{
    internal interface IServerClipboardService
    {
        public Task<ServerClipboardItemResponse[]> GetClipboardItemsAsync();
    }
}
