using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public abstract class Transformation
    {
        public TimeSingle Start { get; set; }
        public TimeSingle End { get; set; }

        public Transformation(TimeSingle start, TimeSingle end) 
        {
            if (start >= end)
            {
                throw new ArgumentException("error: start time should be lower than end time");
            }
            Start = start;
            End = end;
        }

        public List<Transformation> GetWithoutPartialOverlap(List<Transformation> transformations)
        {
            List<TimeSingle> overlapingPoints = [];
            foreach (Transformation otherTransformation in transformations)
            {
                if (otherTransformation.Start > Start && otherTransformation.Start < End)
                {
                    overlapingPoints.Add(otherTransformation.Start);
                }
                if (otherTransformation.End > Start && otherTransformation.End < End)
                {
                    overlapingPoints.Add(otherTransformation.End);
                }
            }

            overlapingPoints.Add(End);
            
            // split on overlaping points
            return SplitOn([.. overlapingPoints.Distinct()]);            
        }

        public abstract List<Transformation> SplitOn(List<TimeSingle> points);

        public abstract List<Vec3> Apply(List<Vec3> oldPositions);
    }
}
