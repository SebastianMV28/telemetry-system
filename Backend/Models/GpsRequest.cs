
namespace Backend.Models
{
    public class GpsRequest
    {
        public string VehicleId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTime Timestamp { get; set; }
    }
}