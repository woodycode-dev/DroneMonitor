using System.Net.Sockets;
using DroneMonitor.Models;

namespace DroneMonitor.Services
{
    public class MavLinkService
    {
        // UDP 클라이언트 (MAVLink는 UDP 통신 사용)
        private UdpClient? udpClient;

        // MAVLink 기본 포트 (Mission Planner / ArduPilot SITL 기본값)
        private const int Port = 14550;

        // 드론 데이터 수신 시 ViewModel에게 알려주는 이벤트
        // ViewModel이 이 이벤트를 구독해서 UI 업데이트
        public event Action<DroneStatus>? OnDroneStatusUpdated;

        // 서비스 시작 - UDP 포트 열고 수신 루프 백그라운드 실행
        public void Start()
        {
            // 14550 포트로 바인딩해서 수신 대기
            udpClient = new UdpClient(Port);
            Task.Run(ReceiveLoop);
        }

        // 서비스 종료 - UDP 연결 닫기
        public void Stop()
        {
            udpClient?.Close();
        }

        // 백그라운드에서 계속 돌면서 UDP 패킷 수신 대기
        private async Task ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    var result = await udpClient!.ReceiveAsync();

                    // 패킷 수신 확인용 로그
                    System.Diagnostics.Debug.WriteLine($"패킷 수신: {result.Buffer.Length} bytes");

                    var status = ParseMavLink(result.Buffer);

                    if (status != null)
                        OnDroneStatusUpdated?.Invoke(status);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"오류: {ex.Message}");
                    break;
                }
            }
        }

        private DroneStatus? ParseMavLink(byte[] data)
        {
            if (data.Length < 10) return null;

            if (data[0] != 0xFD) return null;

            // 서명 여부에 따라 페이로드 오프셋 결정
            byte incompatFlags = data[2];
            bool hasSig = (incompatFlags & 0x01) != 0;
            int o = hasSig ? 25 : 10;

            int msgId = data[7] | (data[8] << 8) | (data[9] << 16);
            System.Diagnostics.Debug.WriteLine($"MSG ID: {msgId}, hasSig: {hasSig}");

            var status = new DroneStatus();

            switch (msgId)
            {
                case 0: // HEARTBEAT
                    status.State = "비행중";
                    return status;

                case 33: // GLOBAL_POSITION_INT
                    if (data.Length < o + 22) return null;
                    // time_boot_ms (4바이트) 스킵
                    status.Latitude = BitConverter.ToInt32(data, o + 4) / 1e7;
                    status.Longitude = BitConverter.ToInt32(data, o + 8) / 1e7;
                    status.Altitude = BitConverter.ToInt32(data, o + 12) / 1000f;
                    status.Speed = (float)Math.Sqrt(
                        Math.Pow(BitConverter.ToInt16(data, o + 18), 2) +
                        Math.Pow(BitConverter.ToInt16(data, o + 20), 2)) / 100f;
                    status.State = "비행중";
                    System.Diagnostics.Debug.WriteLine($"위도: {status.Latitude}, 경도: {status.Longitude}, 고도: {status.Altitude}");
                    return status;

                case 147: // BATTERY_STATUS
                    if (data.Length < o + 36) return null;
                    sbyte remaining = (sbyte)data[o + 35];
                    status.Battery = remaining < 0 ? 0 : remaining;
                    status.State = "비행중";
                    return status;

                default:
                    return null;
            }
        }
    }
}