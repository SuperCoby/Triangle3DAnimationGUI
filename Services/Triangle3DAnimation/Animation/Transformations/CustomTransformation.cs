using GBX.NET;
using TmEssentials;

namespace Triangle3DAnimation.Animation.Transformations
{
    public class CustomTransformation(List<Vec3> translations, TimeSingle start, TimeSingle end) : Transformation(start, end)
    {
		public List<Vec3> Translations { get; set; } = translations;

		public override List<Vec3> Apply(List<Vec3> oldPositions)
        {
            List<Vec3> newPositions = [];
            for (int i = 0; i < Translations.Count; i++) 
            {
                newPositions.Add(oldPositions[i] + Translations[i]);
            }

            return newPositions;
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

                List<Vec3> newTranslations = [];
                for (int i = 0; i < Translations.Count; i++)
                {
                    Vec3 newTranslationVector = (
                        Translations[i].X / (totalDuration / durationOfSplit),
                        Translations[i].Y / (totalDuration / durationOfSplit),
                        Translations[i].Z / (totalDuration / durationOfSplit)
                    );
                    newTranslations.Add(newTranslationVector);
                }
                
                result.Add(new CustomTransformation(newTranslations, startOfSplit, point));
                startOfSplit = point;
            }

            return result;
        }
    }
}
