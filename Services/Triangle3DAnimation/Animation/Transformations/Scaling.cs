using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public class Scaling(float scaleX, float scaleY, float scaleZ, TimeSingle start, TimeSingle end) : Transformation(start, end)
    {
        public float ScaleX { get; set; } = scaleX;
        public float ScaleY { get; set; } = scaleY;
        public float ScaleZ { get; set; } = scaleZ;

        public override List<Transformation> SplitOn(List<TimeSingle> points)
        {
            points.Sort();
            List<Transformation> result = [];
            TimeSingle startOfSplit = Start;
            float scaleX = 1, scaleY = 1, scaleZ = 1;
            foreach (TimeSingle point in points)
            {
                TimeSingle durationOfSplit = point - startOfSplit;
                float normalizedTime = durationOfSplit / (End - startOfSplit);
                float scaleAtPointX = scaleX + (ScaleX - scaleX) * normalizedTime;
                float scaleAtPointY = scaleY + (ScaleY - scaleY) * normalizedTime;
                float scaleAtPointZ = scaleZ + (ScaleZ - scaleZ) * normalizedTime;
                float newScaleFactorX = scaleAtPointX / scaleX;
                float newScaleFactorY = scaleAtPointY / scaleY;
                float newScaleFactorZ = scaleAtPointZ / scaleZ;
                scaleX = scaleAtPointX;
                scaleY = scaleAtPointY;
                scaleZ = scaleAtPointZ;
                result.Add(new Scaling(newScaleFactorX, newScaleFactorY, newScaleFactorZ, startOfSplit, point));
                startOfSplit = point;
            }
            return result;
        }

        public override List<Vec3> Apply(List<Vec3> oldPositions)
        {
            // compute barycenter
            Vec3 barycenter = oldPositions.Aggregate(new Vec3(0, 0, 0), (barycenter, next) => barycenter + (
                next.X / oldPositions.Count,
                next.Y / oldPositions.Count,
                next.Z / oldPositions.Count
                ));
            return oldPositions.ConvertAll(vertexPosition =>
                new Vec3(
                    ((vertexPosition.X - barycenter.X) * ScaleX) + barycenter.X,
                    ((vertexPosition.Y - barycenter.Y) * ScaleY) + barycenter.Y,
                    ((vertexPosition.Z - barycenter.Z) * ScaleZ) + barycenter.Z
                )
            );
        }
    }
}
