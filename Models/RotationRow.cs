using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Triangle3DAnimationGUI.Models
{
	public class RotationRow : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public RotationRow(float startTime = 0, float endTime = 3, float x = 0, float y = 0, float z = 0, int steps = 10)
		{
			_startTime = startTime;
			_endTime = endTime;
			_x = x;
			_y = y;
			_z = z;
			_steps = steps;
		}

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

		private float _x;
		public float X
		{
			get => _x;
			set
			{
				if (_x != value)
				{
					_x = value is float f ? f : 0;
					OnPropertyChanged(nameof(X));
				}
			}
		}

		private float _y;
		public float Y
		{
			get => _y;
			set
			{
				if (_y != value)
				{
					_y = value is float f ? f : 0;
					OnPropertyChanged(nameof(Y));
				}
			}
		}

		private float _z;
		public float Z
		{
			get => _z;
			set
			{
				if (_z != value)
				{
					_z = value is float f ? f : 0;
					OnPropertyChanged(nameof(Z));
				}
			}
		}

		private int _steps;
		public int Steps
		{
			get => _steps;
			set
			{
				if (_steps != value)
				{
					_steps = value is int i ? i : 0;
					OnPropertyChanged(nameof(Steps));
				}
			}
		}
	}
}