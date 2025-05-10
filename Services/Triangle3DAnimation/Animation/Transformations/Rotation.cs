using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public class Rotation(float pitch, float yaw, float roll, int nbSteps, TimeSingle start, TimeSingle end) : Transformation(start, end)
    {

		public float Pitch { get; set; } = pitch;
		public float Yaw { get; set; } = yaw;
		public float Roll { get; set; } = roll;
		public int NbSteps { get; set; } = nbSteps;

		public List<Rotation> getAllRotationsPerStep()
        {
            TimeSingle totalDuration = End - Start;
            TimeSingle stepDuration = totalDuration / NbSteps;
            List<Rotation> result = [];
            TimeSingle startOfStep = Start;
            for (int i = 0; i < NbSteps; i++)
            {
                result.Add(new Rotation(Pitch / NbSteps, Yaw / NbSteps, Roll / NbSteps, 1, startOfStep, startOfStep + stepDuration));
                startOfStep += stepDuration;
            }

            return result;
        }

        public override List<Transformation> SplitOn(List<TimeSingle> points)
        {
            points.Sort();
            List<Transformation> result = [];
            TimeSingle totalDuration = End - Start;
            TimeSingle startOfSplit = Start;
            foreach (TimeSingle point in points)
            {
                TimeSingle durationOfSplit = point - startOfSplit;
                float newPitch = Pitch / (totalDuration / durationOfSplit);
                float newYaw = Yaw / (totalDuration / durationOfSplit);
                float newRoll = Roll / (totalDuration / durationOfSplit);
                result.Add(new Rotation(newPitch, newYaw, newRoll, 1, startOfSplit, point));
                startOfSplit = point;
            }

            return result;
        }

        public override List<Vec3> Apply(List<Vec3> oldPositions)
        {
            Vec3 barycenter = oldPositions.Aggregate(new Vec3(0, 0, 0), (barycenter, next) => barycenter + (
                next.X / oldPositions.Count,
                next.Y / oldPositions.Count,
                next.Z / oldPositions.Count
            ));
            return oldPositions.ConvertAll(vertexPosition => Rotate(vertexPosition - barycenter) + barycenter);
        }

        public Vec3 Rotate(Vec3 vertex)
        {
            var cosa = MathF.Cos(Yaw);
            var sina = MathF.Sin(Yaw);

            var cosb = MathF.Cos(Pitch);
            var sinb = MathF.Sin(Pitch);

            var cosc = MathF.Cos(Roll);
            var sinc = MathF.Sin(Roll);

            var Axx = cosa * cosb;
            var Axy = cosa * sinb * sinc - sina * cosc;
            var Axz = cosa * sinb * cosc + sina * sinc;

            var Ayx = sina * cosb;
            var Ayy = sina * sinb * sinc + cosa * cosc;
            var Ayz = sina * sinb * cosc - cosa * sinc;

            var Azx = -sinb;
            var Azy = cosb * sinc;
            var Azz = cosb * cosc;

            return new Vec3(Axx * vertex.X + Axy * vertex.Y + Axz * vertex.Z, Ayx * vertex.X + Ayy * vertex.Y + Ayz * vertex.Z, Azx * vertex.X + Azy * vertex.Y + Azz * vertex.Z);
        } 
    }
}
