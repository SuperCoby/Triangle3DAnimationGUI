using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public class Identity(TimeSingle start, TimeSingle end) : Transformation(start, end)
    {
		public override List<Vec3> Apply(List<Vec3> oldPositions)
        {
            return oldPositions;
        }

        public override List<Transformation> SplitOn(List<TimeSingle> points)
        {
            points.Sort();
            List<Transformation> result = [];
            // TimeSingle totalDuration = End - Start; // Not used?
            TimeSingle startOfSplit = Start;
            foreach (TimeSingle point in points)
            {
                TimeSingle durationOfSplit = point - startOfSplit;
                result.Add(new Identity(startOfSplit, point));
                startOfSplit = point;
            }

            return result;
        }
    }
}
