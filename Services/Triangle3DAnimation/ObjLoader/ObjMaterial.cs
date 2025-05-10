using System.ComponentModel;

namespace Triangle3DAnimation.ObjLoader
{
	public class ObjMaterial : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Name { get; set; }

		private float _diffuseR;
		public float DiffuseR
		{
			get => _diffuseR;
			set
			{
				var v = Math.Clamp(value, 0f, 1f);
				if (_diffuseR != v)
				{
					_diffuseR = v;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DiffuseR)));
				}
			}
		}

		private float _diffuseG;
		public float DiffuseG
		{
			get => _diffuseG;
			set
			{
				var v = Math.Clamp(value, 0f, 1f);
				if (_diffuseG != v)
				{
					_diffuseG = v;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DiffuseG)));
				}
			}
		}

		private float _diffuseB;
		public float DiffuseB
		{
			get => _diffuseB;
			set
			{
				var v = Math.Clamp(value, 0f, 1f);
				if (_diffuseB != v)
				{
					_diffuseB = v;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DiffuseB)));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.DynamicDependency(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties, typeof(ObjMaterial))]
		public ObjMaterial(string name) { Name = name; }
	}
}