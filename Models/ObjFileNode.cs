namespace Triangle3DAnimationGUI.Models;

public class ObjFileNode
{
    public string Name { get; set; } = string.Empty;
    public string? FullPath { get; set; } // null pour les dossiers
    public List<ObjFileNode> Children { get; set; } = new();
    public bool IsFile => FullPath != null;
}
