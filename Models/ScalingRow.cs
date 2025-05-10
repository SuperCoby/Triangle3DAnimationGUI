using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Triangle3DAnimationGUI.Models
{
	public class ScalingRow : INotifyPropertyChanged
	{
		public ScalingRow(float startTime = 0, float endTime = 3, float scaleX = 1, float scaleY = 1, float scaleZ = 1)
		{
			_startTime = startTime;
			_endTime = endTime;
			_scaleX = scaleX;
			_scaleY = scaleY;
			_scaleZ = scaleZ;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private float _startTime;
		public float StartTime
		{
			get => _startTime;
			set
			{
				if (_startTime != value)
				{
					_startTime = value;
					OnPropertyChanged(nameof(StartTime));
				}
			}
		}

		private float _endTime;
		public float EndTime
		{
			get => _endTime;
			set
			{
				if (_endTime != value)
				{
					_endTime = value;
					OnPropertyChanged(nameof(EndTime));
				}
			}
		}

		private float _scaleX;
		public float ScaleX
		{
			get => _scaleX;
			set
			{
				if (_scaleX != value)
				{
					_scaleX = value is float f ? f : 0;
					OnPropertyChanged(nameof(ScaleX));
				}
			}
		}

		private float _scaleY;
		public float ScaleY
		{
			get => _scaleY;
			set
			{
				if (_scaleY != value)
				{
					_scaleY = value is float f ? f : 0;
					OnPropertyChanged(nameof(ScaleY));
				}
			}
		}

		private float _scaleZ;
		public float ScaleZ
		{
			get => _scaleZ;
			set
			{
				if (_scaleZ != value)
				{
					_scaleZ = value is float f ? f : 0;
					OnPropertyChanged(nameof(ScaleZ));
				}
			}
		}
	}
}