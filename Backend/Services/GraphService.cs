using System;
using System.Collections.Generic;
using System.Text.Json;
using WalkMood.API.Models;

namespace WalkMood.API.Services
{
    public class GraphService : IGraphService
    {
        public Dictionary<long, GraphNode> BuildGraphFromJson(string jsonResponse, string mood)
        {
            var graph = new Dictionary<long, GraphNode>();
            
            // 1. JSON verisini C# nesnelerine dönüştür
            var osmData = JsonSerializer.Deserialize<OsmResponse>(jsonResponse);
            if (osmData == null || osmData.Elements == null) return graph;

            // 2. Önce tüm Düğümleri (Nodes - Kavşaklar/Noktalar) sözlüğe ekle
            foreach (var element in osmData.Elements)
            {
                if (element.Type == "node")
                {
                    graph[element.Id] = new GraphNode
                    {
                        Id = element.Id,
                        Lat = element.Lat,
                        Lon = element.Lon
                    };
                }
            }

            // 3. Yolları (Ways) döngüye al ve Düğümler arasına Kenarlar (Edges) ekle
            foreach (var element in osmData.Elements)
            {
                if (element.Type == "way" && element.Nodes != null && element.Tags != null)
                {
                    // Yolun maliyet katsayısını mod seçimine göre belirle
                    double weightMultiplier = CalculateWeightMultiplier(element.Tags, mood);

                    // Yolun üzerindeki noktaları sırayla birbirine bağla
                    for (int i = 0; i < element.Nodes.Count - 1; i++)
                    {
                        long nodeAId = element.Nodes[i];
                        long nodeBId = element.Nodes[i + 1];

                        // Eğer her iki düğüm de haritamızda (sözlükte) varsa onları birbirine bağla
                        if (graph.TryGetValue(nodeAId, out var nodeA) && graph.TryGetValue(nodeBId, out var nodeB))
                        {
                            double distance = CalculateHaversineDistance(nodeA.Lat, nodeA.Lon, nodeB.Lat, nodeB.Lon);
                            double totalCost = distance * weightMultiplier;

                            // A noktasından B'ye gidiş ekle
                            nodeA.Edges.Add(new GraphEdge { TargetNode = nodeB, PhysicalDistance = distance, Weight = totalCost });
                            // B noktasından A'ya dönüş ekle (Yollar genelde çift yönlü yürünebilir)
                            nodeB.Edges.Add(new GraphEdge { TargetNode = nodeA, PhysicalDistance = distance, Weight = totalCost });
                        }
                    }
                }
            }

            return graph;
        }

        // --- YARDIMCI METOTLAR ---

        // Seçilen moda ve yolun OSM etiketlerine göre maliyet katsayısını belirler
        private double CalculateWeightMultiplier(Dictionary<string, string> tags, string mood)
        {
            double multiplier = 1.0; // Standart yol

            if (mood == "nature")
            {
                if (tags.ContainsKey("leisure") && tags["leisure"] == "park") multiplier = 0.4; // Çok cazip
                else if (tags.ContainsKey("natural") && tags["natural"] == "wood") multiplier = 0.5;
                else if (tags.ContainsKey("highway") && tags["highway"] == "pedestrian") multiplier = 0.8;
                else if (tags.ContainsKey("highway") && (tags["highway"] == "primary" || tags["highway"] == "secondary")) multiplier = 2.0; // Ağaçsız ana yol cezası
            }
            else if (mood == "safe")
            {
                if (tags.ContainsKey("highway") && tags["highway"] == "residential") multiplier = 0.6; // Sakin mahalle
                else if (tags.ContainsKey("highway") && tags["highway"] == "living_street") multiplier = 0.5; // Yayalara özel
                else if (tags.ContainsKey("traffic_calming") && tags["traffic_calming"] == "yes") multiplier = 0.7;
                else if (tags.ContainsKey("highway") && (tags["highway"] == "primary" || tags["highway"] == "secondary")) multiplier = 5.0; // Büyük cadde cezası (tehlikeli)
            }

            return multiplier;
        }

        // İki koordinat (Enlem/Boylam) arasındaki fiziksel mesafeyi metre cinsinden hesaplar
        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Dünya'nın yarıçapı (metre)
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}