using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Zaclip.Dtos;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.LocalClipboardService;
using Zaclip.States;

namespace Zaclip.Services.ServerClipboardService
{
    public class ServerClipboardService : IServerClipboardService
    {
        private readonly HttpClient _http;
        private readonly ILocalClipboardService _localClipboardService;
        private readonly SessionContext _session;

        public ServerClipboardService(HttpClient http, ILocalClipboardService local, SessionContext session)
        {
            _http = http;
            _localClipboardService = local;
            _session = session;
        }

        public async Task<ServerClipboardItemResponse[]> GetClipboardItemsAsync(int skip, int take)
        {
            if(!_session.IsLoggedIn)
                return Array.Empty<ServerClipboardItemResponse>();

            var result = await _http.GetAsync($"api/clipboarditem?skip={skip}&take={take}");
            if (!result.IsSuccessStatusCode)
                throw new Exception("Failed to get server clipboard items.");

            return await result.Content.ReadFromJsonAsync<ServerClipboardItemResponse[]>() ?? Array.Empty<ServerClipboardItemResponse>();
        }

        public async Task CreateClipboardItemAsync(int itemId)
        {
            if(!_session.IsLoggedIn)
                throw new InvalidOperationException("User is not logged in.");

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
            if(!_session.IsLoggedIn)
                return new List<Guid>();

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
