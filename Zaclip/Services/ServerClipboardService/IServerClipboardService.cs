using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.ClipboardItemsService
{
    public interface IServerClipboardService
    {
        public Task<ServerClipboardItemResponse[]> GetClipboardItemsAsync(int skip, int take);
        public Task CreateClipboardItemAsync(int itemId);
        public Task<List<Guid>> GetExistingGuidsAsync(List<Guid> idList);
    }
}
