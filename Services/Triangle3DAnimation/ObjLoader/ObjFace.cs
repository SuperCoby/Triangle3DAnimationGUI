using GBX.NET;

namespace Triangle3DAnimation.ObjLoader
{
    public class ObjFace(ObjFaceVertex x, ObjFaceVertex y, ObjFaceVertex z, ObjMaterial? material)
    {
        public ObjFaceVertex V1 { get; set; } = x;
        public ObjFaceVertex V2 { get; set; } = y;
        public ObjFaceVertex V3 { get; set; } = z;
        public ObjMaterial? Material { get; set; } = material;

        public override string ToString()
        {
            return "face with material : " + (Material == null ? " " : Material.Name);
        }

        public Int3 GetTriangleVerticesInInt3()
        {
            // obj format uses 1-based indexing but media tracker triangles uses 0-based indexing
            return new Int3(V1.Vertex.Index - 1, V2.Vertex.Index - 1, V3.Vertex.Index - 1);
        }

        public IEnumerable<ObjFaceVertex> Vertices => [V1, V2, V3];
    }
}
