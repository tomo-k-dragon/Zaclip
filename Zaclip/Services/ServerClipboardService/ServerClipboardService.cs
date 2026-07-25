using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Zaclip.Dtos;
using Zaclip.Services.ClipboardItemsService;

namespace Zaclip.Services.ServerClipboardService
{
    public class ServerClipboardService : IServerClipboardService
    {
        private HttpClient _http;
        public ServerClipboardService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ServerClipboardItemResponse[]> GetClipboardItemsAsync()
        {
            var result = await _http.GetAsync("api/clipboarditem"); 
            if (!result.IsSuccessStatusCode)
                throw new Exception("Failed to get server clipboard items.");

            return await result.Content.ReadFromJsonAsync<ServerClipboardItemResponse[]>() ?? Array.Empty<ServerClipboardItemResponse>();
        }
    }
}
