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
            // .NET'in isteği iptal etmeden önce bekleme süresini 60 saniyeye çıkarıyoruz
            _httpClient.Timeout = TimeSpan.FromSeconds(60); 
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WalkMood_BitirmeProjesi");
        }

        public async Task<string> GetRouteDataAsync(double startLat, double startLng, double endLat, double endLng, string mood)
        {
            // 1. Alanı ÇOK daha fazla daraltıyoruz (Veri yükünü hafifletmek için padding 0.002 yapıldı)
            double padding = 0.002; 
            double minLat = Math.Min(startLat, endLat) - padding;
            double minLng = Math.Min(startLng, endLng) - padding;
            double maxLat = Math.Max(startLat, endLat) + padding;
            double maxLng = Math.Max(startLng, endLng) + padding;

            // 2. Overpass'e "Bu işlemi bitirmek için 50 saniyen var" diyoruz
            string query = $@"[out:json][timeout:50];
            (
              way({minLat.ToString(CultureInfo.InvariantCulture)},{minLng.ToString(CultureInfo.InvariantCulture)},{maxLat.ToString(CultureInfo.InvariantCulture)},{maxLng.ToString(CultureInfo.InvariantCulture)})[highway];
            );
            (._;>;);
            out body;";

            // 3. Çok daha stabil olan alternatif sunucuyu (Kumi Systems) veya Z-Mirror'ı kullanıyoruz
            // Eğer Kumi de yanıt vermezse burayı "https://z.overpass-api.de/api/interpreter..." olarak değiştirebilirsin
            var requestUrl = $"https://overpass.kumi.systems/api/interpreter?data={Uri.EscapeDataString(query)}";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); // Eğer 200 dönmezse direkt Catch bloğuna düşer
                
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // Sunucu çökerse ekrana devasa kırmızı hatalar basmak yerine temiz bir mesaj fırlatıyoruz
                throw new Exception($"Harita sunucuları şu an yanıt vermiyor. Lütfen 1 dakika sonra tekrar deneyin. Detay: {ex.Message}");
            }
        }
    }
}