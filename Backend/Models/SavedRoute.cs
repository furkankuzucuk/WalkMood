using System;

namespace WalkMood.API.Models
{
    public class SavedRoute
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; } // İlişkisel bağlantı (Foreign Key)
        public string StartLocationName { get; set; } = string.Empty;
        public string EndLocationName { get; set; } = string.Empty;
        public string MoodType { get; set; } = string.Empty; // Örn: "Nature" veya "Safe"
        public decimal TotalDistanceKm { get; set; }
        public string RouteGeoJson { get; set; } = string.Empty; // Harita çizim verisi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}