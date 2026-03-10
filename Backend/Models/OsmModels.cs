using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WalkMood.API.Models
{
    public class OsmResponse
    {
        [JsonPropertyName("elements")]
        public List<OsmElement> Elements { get; set; } = new List<OsmElement>();
    }

    public class OsmElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        // Eğer bu bir "way" (yol) ise, içindeki düğümlerin (node) ID listesi
        [JsonPropertyName("nodes")]
        public List<long>? Nodes { get; set; }

        // Etiketler: Bu yolun bir park mı, ana cadde mi yoksa güvenli sokak mı olduğunu söyler
        [JsonPropertyName("tags")]
        public Dictionary<string, string>? Tags { get; set; }
    }
}