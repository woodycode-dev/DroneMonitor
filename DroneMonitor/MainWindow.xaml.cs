using DroneMonitor.ViewModels;
using System.Windows;

namespace DroneMonitor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // MainViewModel을 DataContext로 설정
            DataContext = new MainViewModel();
        }
    }
}