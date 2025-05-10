namespace Triangle3DAnimation.ObjLoader
{
    public class ObjModelBuilder
    {
        public List<ObjVertex> Vertices { get; set; }
        public List<ObjFace> Faces { get; set; }
        public List<ObjMaterial> Materials { get; set; }
        public ObjMaterial? CurrentMaterial { get; set; }
        public int VertexCount;
        public int TextureVertexCount;
        public List<List<ObjFaceVertex>> OriginalFaces { get; set; } = new();
        private Dictionary<int, ObjVertex> VertexByIndex = new();

        public ObjModelBuilder()
        {
            Vertices = [];
            Materials = [];
            Faces = [];
            VertexCount = 0;
            TextureVertexCount = 0;
        }

        public void AddVertex(float x, float y, float z)
        {
            VertexCount++;
            var v = new ObjVertex(x, y, z, VertexCount);
            Vertices.Add(v);
            VertexByIndex[VertexCount] = v;
        }

        public void AddFace(List<ObjFaceVertex> faceVertices)
        {
            OriginalFaces.Add(new List<ObjFaceVertex>(faceVertices));
            if (faceVertices.Count == 3)
            {
                Faces.Add(new ObjFace(faceVertices[0], faceVertices[1], faceVertices[2], CurrentMaterial));
            }
            else if (faceVertices.Count > 3) { }
            {
                for (int i = 1; i < faceVertices.Count - 1; i++)
                {
                    Faces.Add(new ObjFace(faceVertices[0], faceVertices[i], faceVertices[i + 1], CurrentMaterial));
                }
            }
        }

        public ObjVertex GetVertex(int index)
        {
            if (VertexByIndex.TryGetValue(index, out var v))
                return v;
            throw new ArgumentException($"error : vertex at index {index} not found");
        }

        public void setCurrentMaterial(string materialName)
        {
            foreach (ObjMaterial objMaterial in Materials)
            {
                if (objMaterial.Name.Equals(materialName))
                {
                    CurrentMaterial = objMaterial;
                }
            }
        }

        public ObjModel Build()
        {
            return new ObjModel(Vertices, Faces, Materials, OriginalFaces);
        }
    }
}
