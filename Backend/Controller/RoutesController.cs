using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WalkMood.API.Data;
using WalkMood.API.DTOs;
using WalkMood.API.Services;

namespace WalkMood.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly WalkMoodDbContext _context;
        private readonly IOsmService _osmService; // Servisimizi buraya dahil ediyoruz

        public RoutesController(WalkMoodDbContext context, IOsmService osmService)
        {
            _context = context;
            _osmService = osmService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteRequestDto request)
        {
            // Adım 1: Overpass API'den bölgenin harita verilerini (JSON) çek
            var osmJsonData = await _osmService.GetRouteDataAsync(
                request.StartLat, request.StartLng, 
                request.EndLat, request.EndLng, 
                request.Mood);

            // TODO: Adım 2 - İndirilen JSON verisi ayrıştırılacak (Parsing)
            // TODO: Adım 3 - Dijkstra/A* algoritması ile modlara (Doğa/Güvenli) göre rotalar çizilecek

            // Şimdilik API'nin başarıyla haritayı indirdiğini görmek için JSON verisinin ilk 500 karakterini dönüyoruz
            return Ok(new {
                Message = "Harita verisi Overpass'ten başarıyla indirildi!",
                DataPreview = osmJsonData.Substring(0, Math.Min(osmJsonData.Length, 500)) + "..."
            });
        }
    }
}