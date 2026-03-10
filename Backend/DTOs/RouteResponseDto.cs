using System.Collections.Generic;

namespace WalkMood.API.DTOs
{
    public class RouteResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public double TotalDistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        
        // Haritada çizgiyi çekebilmemiz için sıralı koordinat listesi: [[lat, lng], [lat, lng]...]
        public List<double[]> RouteCoordinates { get; set; } = new List<double[]>();
    }
}