using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Zaclip.Dtos;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.LocalClipboardService;

namespace Zaclip.Services.ServerClipboardService
{
    public class ServerClipboardService : IServerClipboardService
    {
        private readonly HttpClient _http;
        private readonly ILocalClipboardService _localClipboardService;

        public ServerClipboardService(HttpClient http, ILocalClipboardService local)
        {
            _http = http;
            _localClipboardService = local;
        }

        public async Task<ServerClipboardItemResponse[]> GetClipboardItemsAsync(int skip, int take)
        {
            var result = await _http.GetAsync($"api/clipboarditem?skip={skip}&take={take}");
            if (!result.IsSuccessStatusCode)
                throw new Exception("Failed to get server clipboard items.");

            return await result.Content.ReadFromJsonAsync<ServerClipboardItemResponse[]>() ?? Array.Empty<ServerClipboardItemResponse>();
        }

        public async Task CreateClipboardItemAsync(int itemId)
        {
            var item = await _localClipboardService.GetItemAsync(itemId);
            if (item == null)
                throw new InvalidOperationException($"Clipboard item not found: {itemId}");

            var body = new ClipboardItemRequest
            {
                Guid = item.Guid,
                Content = item.Content,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
            var resp = await _http.PostAsJsonAsync("api/clipboarditem/", body);
            if (!resp.IsSuccessStatusCode)
                throw new Exception("Failed to post clipboard item.");
        }

        public async Task<List<Guid>> GetExistingGuidsAsync(List<Guid> guids)
        {
            var request = new ClipboardItemExistenceRequest
            {
                Guids = guids
            };
            var response = await _http.PostAsJsonAsync(
                "api/clipboarditem/existence",
                request);

            response.EnsureSuccessStatusCode();
            var result = await response.Content
                .ReadFromJsonAsync<ClipboardItemExistenceResponse>();
            return result?.ExistingGuids ?? [];
        }
    }
}
