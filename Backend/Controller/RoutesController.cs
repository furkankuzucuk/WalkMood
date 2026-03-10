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
        private readonly IOsmService _osmService;
        private readonly IGraphService _graphService;
        private readonly IRoutingService _routingService;

        public RoutesController(
            WalkMoodDbContext context, 
            IOsmService osmService, 
            IGraphService graphService, 
            IRoutingService routingService)
        {
            _context = context;
            _osmService = osmService;
            _graphService = graphService;
            _routingService = routingService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateRoute([FromBody] RouteRequestDto request)
        {
            // 1. Overpass API'den bölgenin harita verilerini (JSON) çek
            var osmJsonData = await _osmService.GetRouteDataAsync(
                request.StartLat, request.StartLng, 
                request.EndLat, request.EndLng, 
                request.Mood);

            // 2. JSON verisini Yön Bulma Ağına (Graph) Çevir ve Maliyetleri Hesapla
            var graph = _graphService.BuildGraphFromJson(osmJsonData, request.Mood);

            if (graph.Count == 0)
            {
                return BadRequest(new { Message = "Bu bölge için harita verisi işlenemedi." });
            }

            // 3. Dijkstra Algoritması ile En İyi Rotayı Bul
            var routeResult = _routingService.FindOptimalRoute(
                graph, 
                request.StartLat, request.StartLng, 
                request.EndLat, request.EndLng);

            return Ok(routeResult);
        }
    }
}