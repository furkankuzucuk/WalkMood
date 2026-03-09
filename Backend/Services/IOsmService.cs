using System.Threading.Tasks;

namespace WalkMood.API.Services
{
    public interface IOsmService
    {
        Task<string> GetRouteDataAsync(double startLat, double startLng, double endLat, double endLng, string mood);
    }
}