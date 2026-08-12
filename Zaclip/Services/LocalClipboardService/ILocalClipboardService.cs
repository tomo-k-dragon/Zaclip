using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;
using Zaclip.Models;

namespace Zaclip.Services.LocalClipboardService
{
    public interface ILocalClipboardService
    {
        public Task<List<ClipboardItem>> GetAsync(ClipboardQuery query);
        public Task<ClipboardItem?> GetItemAsync(int itemId);
        public Task<ClipboardItem> SaveTemporaryAsync(string itemText);
        public Task PersistAsync(int itemId);
        public Task DeleteAsync(int itemId);
        public Task DeleteTemporaryAsync();
        public Task<List<Guid>> GetExistGuidsAsync(List<Guid> idList);
    }
}
