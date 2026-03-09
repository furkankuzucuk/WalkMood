using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace WalkMood.API.Services
{
    public class OsmService : IOsmService
    {
        private readonly HttpClient _httpClient;

        public OsmService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Overpass API, bot saldırılarını engellemek için User-Agent başlığı ister
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WalkMood_BitirmeProjesi");
        }

        public async Task<string> GetRouteDataAsync(double startLat, double startLng, double endLat, double endLng, string mood)
        {
            // 1. Bounding Box (Arama Çerçevesi) Hesaplama
            // Harita alanını daraltarak (0.005) sunucunun daha hızlı yanıt vermesini sağlıyoruz
            double padding = 0.005; 
            double minLat = Math.Min(startLat, endLat) - padding;
            double minLng = Math.Min(startLng, endLng) - padding;
            double maxLat = Math.Max(startLat, endLat) + padding;
            double maxLng = Math.Max(startLng, endLng) + padding;

            // 2. Overpass QL (Sorgu Dili)
            // [timeout:25] ekleyerek sunucuya bu sorguyu en fazla 25 saniyede bitirmesini söylüyoruz
            string query = $@"[out:json][timeout:25];
            (
              way({minLat.ToString(CultureInfo.InvariantCulture)},{minLng.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLng.ToString(CultureInfo.InvariantCulture)})[highway];
            );
            (._;>;);
            out body;";

            // 3. İsteği At ve JSON Yanıtını Al (Daha hızlı olan lz4 ayna sunucusunu kullanıyoruz)
            var requestUrl = $"https://lz4.overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}