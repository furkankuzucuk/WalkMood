using System;
using System.Collections.Generic;
using System.Linq;
using WalkMood.API.Models;
using WalkMood.API.DTOs;

namespace WalkMood.API.Services
{
    public class RoutingService : IRoutingService
    {
        public RouteResponseDto FindOptimalRoute(Dictionary<long, GraphNode> graph, double startLat, double startLng, double endLat, double endLng)
        {
            var response = new RouteResponseDto();

            // 1. Kullanıcının tıkladığı koordinatlara en yakın harita düğümlerini (kavşakları) bul
            var startNode = FindNearestNode(graph, startLat, startLng);
            var endNode = FindNearestNode(graph, endLat, endLng);

            if (startNode == null || endNode == null)
            {
                response.Message = "Hata: Seçilen bölgede geçerli bir yol bulunamadı.";
                return response;
            }

            // 2. Dijkstra Algoritması İçin Hazırlık
            var distances = new Dictionary<long, double>(); // Başlangıca olan en ucuz maliyet
            var previousNodes = new Dictionary<long, GraphNode>(); // Rotayı geri çizmek için
            var priorityQueue = new PriorityQueue<long, double>(); // En ucuz düğümü seçmek için kuyruk

            foreach (var node in graph.Values)
            {
                distances[node.Id] = double.MaxValue;
            }

            distances[startNode.Id] = 0;
            priorityQueue.Enqueue(startNode.Id, 0);

            // 3. Dijkstra Ana Döngüsü
            while (priorityQueue.Count > 0)
            {
                var currentNodeId = priorityQueue.Dequeue();

                // Hedefe ulaştıysak aramayı bitir
                if (currentNodeId == endNode.Id) break;

                var currentNode = graph[currentNodeId];

                foreach (var edge in currentNode.Edges)
                {
                    var neighbor = edge.TargetNode;
                    // Yeni maliyet = Şu anki maliyet + yolun katsayılı ağırlığı
                    double newCost = distances[currentNodeId] + edge.Weight;

                    if (newCost < distances[neighbor.Id])
                    {
                        distances[neighbor.Id] = newCost;
                        previousNodes[neighbor.Id] = currentNode;
                        priorityQueue.Enqueue(neighbor.Id, newCost);
                    }
                }
            }

            // 4. Hedef bulunamadıysa dön
            if (!previousNodes.ContainsKey(endNode.Id))
            {
                response.Message = "Hata: Başlangıç ve bitiş noktası arasında kesintisiz bir yol bulunamadı.";
                return response;
            }

            // 5. Rotayı Geriye Doğru Oluşturma (Reconstruction)
            var path = new List<GraphNode>();
            var step = endNode;
            
            while (step != null)
            {
                path.Add(step);
                previousNodes.TryGetValue(step.Id, out step!);
            }
            path.Reverse(); // Başlangıçtan bitişe doğru çevir

            // 6. Sonuçları Hesapla ve DTO'ya Aktar
            double totalPhysicalDistanceMetres = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                var connectingEdge = current.Edges.First(e => e.TargetNode.Id == next.Id);
                totalPhysicalDistanceMetres += connectingEdge.PhysicalDistance;

                response.RouteCoordinates.Add(new double[] { current.Lat, current.Lon });
            }
            response.RouteCoordinates.Add(new double[] { path.Last().Lat, path.Last().Lon });

            response.TotalDistanceKm = Math.Round(totalPhysicalDistanceMetres / 1000.0, 2);
            response.EstimatedTimeMinutes = (int)Math.Ceiling(response.TotalDistanceKm / 5.0 * 60); // Ortalama yürüme hızı 5 km/s
            response.Message = "Rota başarıyla oluşturuldu!";

            return response;
        }

        // --- YARDIMCI METOT: Verilen koordinata haritadaki en yakın düğümü bulur ---
        private GraphNode? FindNearestNode(Dictionary<long, GraphNode> graph, double lat, double lng)
        {
            GraphNode? nearestNode = null;
            double minDistance = double.MaxValue;

            foreach (var node in graph.Values)
            {
                // Basit bir Öklid mesafesi karşılaştırması (yakın nokta bulmak için yeterlidir)
                double dLat = node.Lat - lat;
                double dLng = node.Lon - lng;
                double distanceSq = (dLat * dLat) + (dLng * dLng);

                if (distanceSq < minDistance)
                {
                    minDistance = distanceSq;
                    nearestNode = node;
                }
            }

            return nearestNode;
        }
    }
}