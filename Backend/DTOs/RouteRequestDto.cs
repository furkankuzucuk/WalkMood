namespace WalkMood.API.DTOs
{
    public class RouteRequestDto
    {
    // baslangıc koordinat
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        
    // bitis koordinat
        public double EndLat { get; set; }
        public double EndLng { get; set; }
        
        // rota istegi
        public string Mood { get; set; } = string.Empty; 
    }
}