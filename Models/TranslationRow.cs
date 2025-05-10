using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Triangle3DAnimationGUI.Models
{
	public class TranslationRow : INotifyPropertyChanged
	{
		public TranslationRow(float startTime = 0, float endTime = 3, float x = 0, float y = 0, float z = 0)
		{
			_startTime = startTime;
			_endTime = endTime;
			_x = x;
			_y = y;
			_z = z;
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
	}
}
