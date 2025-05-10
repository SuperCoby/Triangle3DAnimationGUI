using System.Collections.ObjectModel;

namespace Triangle3DAnimation.ObjLoader
{
    public class ObjModel(List<ObjVertex> vertices, List<ObjFace> faces, List<ObjMaterial> materials, List<List<ObjFaceVertex>>? originalFaces = null)
    {
        public List<ObjVertex> Vertices { get; set; } = vertices;
        public List<ObjFace> Faces { get; set; } = faces;
        public ObservableCollection<ObjMaterial> Materials { get; set; } = new(materials);
        public List<List<ObjFaceVertex>> OriginalFaces { get; set; } = originalFaces ?? new List<List<ObjFaceVertex>>();

        public override string ToString()
        {
            string toReturn = "ObjModel :\n";
            toReturn += "vertices :\n";
            foreach (ObjVertex vertex in Vertices)
            {
                toReturn += vertex.ToString() + "\n";
            }
            toReturn += "faces :\n";
            foreach (ObjFace face in Faces)
            {
                toReturn += face.ToString() + "\n";
            }
            return toReturn;
        }
    }
}
