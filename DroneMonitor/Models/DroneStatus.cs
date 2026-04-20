
namespace DroneMonitor.Models
{
    public class DroneStatus
    {
        public float Battery { get; set; }      // 배터리 (%)
        public float Altitude { get; set; }     // 고도 (m)
        public float Speed { get; set; }        // 속도 (m/s)
        public double Latitude { get; set; }    // GPS 위도
        public double Longitude { get; set; }   // GPS 경도
        public string State { get; set; } = ""; // 드론 상태
    }
}
