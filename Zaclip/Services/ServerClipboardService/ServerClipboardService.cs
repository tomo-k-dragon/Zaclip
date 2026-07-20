using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.ServerClipboardService
{
    public class ServerClipboardService
    {
        private HttpClient _http;
        public ServerClipboardService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ServerClipboardItemResponse[]> GetServerClipboardItemsAsync()
        {
            var result = await _http.GetAsync("http://localhost:60262/api/clipboarditem"); 
            if (!result.IsSuccessStatusCode)
                throw new Exception("Failed to get server clipboard items.");

            return await result.Content.ReadFromJsonAsync<ServerClipboardItemResponse[]>() ?? Array.Empty<ServerClipboardItemResponse>();
        }
    }
}
