using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public class Translation(Vec3 translationVector, TimeSingle start, TimeSingle end) : Transformation(start, end)
    {
		public Vec3 TranslationVector { get; set; } = translationVector;

		public override List<Transformation> SplitOn(List<TimeSingle> points)
        {
            points.Sort();
            List<Transformation> result = [];
            TimeSingle totalDuration = End - Start;
            TimeSingle startOfSplit = Start;
            foreach (TimeSingle point in points)
            {
                TimeSingle durationOfSplit = point - startOfSplit;
                Vec3 newTranslationVector = (
                    TranslationVector.X / (totalDuration / durationOfSplit),
                    TranslationVector.Y / (totalDuration / durationOfSplit),
                    TranslationVector.Z / (totalDuration / durationOfSplit)
                );
                result.Add(new Translation(newTranslationVector, startOfSplit, point));
                startOfSplit = point;
            }

            return result;
        }

        public override List<Vec3> Apply(List<Vec3> oldPositions)
        {
            return oldPositions.ConvertAll(vertexPosition => vertexPosition + TranslationVector);
        }
    }
}
