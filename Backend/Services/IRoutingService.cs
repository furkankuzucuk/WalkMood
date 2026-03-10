using System.Collections.Generic;
using WalkMood.API.Models;
using WalkMood.API.DTOs;

namespace WalkMood.API.Services
{
    public interface IRoutingService
    {
        RouteResponseDto FindOptimalRoute(Dictionary<long, GraphNode> graph, double startLat, double startLng, double endLat, double endLng);
    }
}