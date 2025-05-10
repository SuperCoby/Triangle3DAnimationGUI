namespace Triangle3DAnimation.ObjLoader
{
    public class ObjAnimation
    {
        public Dictionary<int, ObjModel> Frames { get; set; } 

        public ObjAnimation() 
        {
            Frames = [];
        }
    }
}
