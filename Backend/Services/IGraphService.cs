using System.Collections.Generic;
using WalkMood.API.Models;
namespace WalkMood.API.Services
{
    public interface IGraphService
    {
        Dictionary<long , GraphNode> BuildGraphFromJson(string jsonResponse, string mood);
    }
}