using GBX.NET;

namespace Triangle3DAnimation.ObjLoader
{
    public class ObjVertex(float x, float y, float z, int index)
	{
		public float X { get; private set; } = x;

		public float Y { get; private set; } = y;

		public float Z { get; private set; } = z;

		public int Index { get; private set; } = index;

		public override string ToString()
        {
            return X + ", " + Y + ", " + Z + ", " + Index;
        }

        public Vec3 ToVec3() 
        {
            return new Vec3(X, Y, Z);   
        }
    }
}
