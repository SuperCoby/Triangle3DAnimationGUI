namespace Triangle3DAnimationGUI.Models
{
	public class ProjectSettings
	{
		public float? OriginX { get; set; }
		public float? OriginY { get; set; }
		public float? OriginZ { get; set; }
		public List<TranslationRow> TranslationRows { get; set; } = [];
		public List<RotationRow> RotationRows { get; set; } = [];
		public List<ScalingRow> ScalingRows { get; set; } = [];
		public List<OrbitRow> OrbitRows { get; set; } = [];
		public float ShadingIntensity { get; set; }
		public List<Triangle3DAnimation.ObjLoader.ObjMaterial> Materials { get; set; } = new();
		public string? ObjPath { get; set; }
		public string? GbxPath { get; set; }
		public bool IsTranslationAnimation { get; set; }
		public bool IsScalingAnimation { get; set; }
		public bool IsRotationAnimation { get; set; }
		public bool IsOrbitEnabled { get; set; }
	}
}
