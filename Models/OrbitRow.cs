using System.ComponentModel;

namespace Triangle3DAnimationGUI.Models
{
    public class OrbitRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public OrbitRow(float startTime = 0, float endTime = 3, float radius = 2, int steps = 60, float degrees = 360)
        {
            _startTime = startTime;
            _endTime = endTime;
            _radius = radius;
            _steps = steps;
            _degrees = degrees;
        }

        private float _startTime;
        public float StartTime
        {
            get => _startTime;
            set { if (_startTime != value) { _startTime = value; OnPropertyChanged(nameof(StartTime)); } }
        }

        private float _endTime;
        public float EndTime
        {
            get => _endTime;
            set { if (_endTime != value) { _endTime = value; OnPropertyChanged(nameof(EndTime)); } }
        }

        private float _radius;
        public float Radius
        {
            get => _radius;
            set { if (_radius != value) { _radius = value; OnPropertyChanged(nameof(Radius)); } }
        }

        private int _steps;
        public int Steps
        {
            get => _steps;
            set { if (_steps != value) { _steps = value; OnPropertyChanged(nameof(Steps)); } }
        }

        private float _degrees = 360;
        public float Degrees
        {
            get => _degrees;
            set { if (_degrees != value) { _degrees = value; OnPropertyChanged(nameof(Degrees)); } }
        }
    }
}
