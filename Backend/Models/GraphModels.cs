using System.Collections.Generic;

namespace WalkMood.API.Models
{
    // Haritadaki bir kavşak veya nokta
    public class GraphNode
    {
        public long Id { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public List<GraphEdge> Edges { get; set; } = new List<GraphEdge>();
    }

    // İki nokta arasındaki yol (Kenar)
    public class GraphEdge
    {
        public GraphNode TargetNode { get; set; } = null!;
        public double PhysicalDistance { get; set; } // Gerçek mesafe (metre)
        
        // Burası projenin kalbi: Doğa veya Güvenlik moduna göre bu maliyet (weight) değişecek!
        public double Weight { get; set; } 
    }
}