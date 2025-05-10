using System.Text.Json.Serialization;
using Triangle3DAnimationGUI.Models;
using Triangle3DAnimationGUI.ViewModels;

namespace Triangle3DAnimationGUI.Json;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProjectSettings))]
[JsonSerializable(typeof(List<TranslationRow>))]
[JsonSerializable(typeof(List<RotationRow>))]
[JsonSerializable(typeof(List<ScalingRow>))]
[JsonSerializable(typeof(List<OrbitRow>))]
[JsonSerializable(typeof(List<Triangle3DAnimation.ObjLoader.ObjMaterial>))]
public partial class ProjectSettingsJsonContext : JsonSerializerContext
{
}
