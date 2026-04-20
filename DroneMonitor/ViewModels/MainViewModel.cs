using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DroneMonitor.Models;
using DroneMonitor.Services;

namespace DroneMonitor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly MavLinkService _mavLinkService;

        [ObservableProperty]
        private float battery;

        [ObservableProperty]
        private float altitude;

        [ObservableProperty]
        private float speed;

        [ObservableProperty]
        private double latitude;

        [ObservableProperty]
        private double longitude;

        [ObservableProperty]
        private string state = "연결 대기중...";

        [ObservableProperty]
        private bool isConnected = false;

        public MainViewModel()
        {
            _mavLinkService = new MavLinkService();

            // 드론 데이터 수신 시 UI 업데이트
            _mavLinkService.OnDroneStatusUpdated += UpdateFromDroneStatus;
        }

        // 연결 버튼
        [RelayCommand]
        private void Connect()
        {
            // 이미 연결된 경우 중복 연결 방지
            if (IsConnected) return;

            _mavLinkService.Start();
            IsConnected = true;
            State = "연결됨";
        }

        // 연결 해제 버튼
        [RelayCommand]
        private void Disconnect()
        {
            _mavLinkService.Stop();
            IsConnected = false;
            State = "연결 해제됨";
        }

        private void UpdateFromDroneStatus(DroneStatus status)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // 0이 아닌 값만 업데이트 (메시지마다 부분 업데이트)
                if (status.Battery != 0) Battery = status.Battery;
                if (status.Altitude != 0) Altitude = status.Altitude;
                if (status.Speed != 0) Speed = status.Speed;
                if (status.Latitude != 0) Latitude = status.Latitude;
                if (status.Longitude != 0) Longitude = status.Longitude;
                if (!string.IsNullOrEmpty(status.State)) State = status.State;
            });
        }
    }
}