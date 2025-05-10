using GBX.NET;
using GBX.NET.Engines.Game;
using TmEssentials;
using Triangle3DAnimation.Animation.Transformations;

namespace Triangle3DAnimation.Animation
{
    /*
     * Animation based on a single 3D model. A single block of media tracker will be used to represent it,
     * meaning only the positions of vertices can change during the animation. Colors can't change and it's 
     * impossible to introduce new vertices/new triangles.
     */
    public class SingleBlockTriangleAnimation(Base baseModel) : TriangleAnimation()
    {

        public List<Vec4> VerticesColor { get; set; } = [];

        public List<Int3> Triangles { get; set; } = [];

        public List<AnimationFrame> AnimationFrames { get; set; } = [];

        public List<Transformation> Transformations { get; set; } = [];

        public Base BaseOfAnimation { get; set; } = baseModel;

        public TimeSingle Start { get; set; } = TimeSingle.FromSeconds(0);

        public TimeSingle? End { get; set; }

        public void AddTransformation(Transformation transformation)
        {
            if (End == null || End < transformation.End)
            {
                End = transformation.End;
            }

            // subdivide all rotations            
            if (transformation is Rotation rotation)
            {
                Transformations.AddRange(rotation.getAllRotationsPerStep());
            }
            else
            {
                Transformations.Add(transformation);
            }
        }

        public void GenerateFrames()
        {
            AnimationFrames.Clear();
            // init colors and faces
            BaseOfAnimation.InitAnimation(this);
            AnimationFrames.Add(BaseOfAnimation.GetFirstFrame(Start));

            if (BaseOfAnimation is BaseAnimation animation)
            {
                animation.GenerateAllTransformations(Start, End).ForEach(transformation =>
                {
                    AddTransformation(transformation);
                });
            }

            if (Transformations.Count <= 0)
            {
                return;
            }

            // remove all partial overlap
            List<Transformation> transformationsWithoutPartialOverlap = [];
            foreach (Transformation transformation in Transformations)
            {
                transformationsWithoutPartialOverlap.AddRange(transformation.GetWithoutPartialOverlap(Transformations));
            }

            // merge transformations that fully overlap
            List<List<Transformation>> orderedAndGroupedTransformations = OrderAndGroupTransformations(transformationsWithoutPartialOverlap);

            // apply all the transformations
            AnimationFrame baseFrame = AnimationFrames[0];
            AnimationFrame lastFrame = baseFrame;
            List<Transformation> previousTransformations = [];
            TimeSingle lastTransformationEndTime = Start;
            foreach (List<Transformation> frameTransformations in orderedAndGroupedTransformations)
            {
                if (frameTransformations[0].Start != lastTransformationEndTime)
                {
                    AnimationFrames.Add(new AnimationFrame(lastFrame.VerticesPositions, frameTransformations[0].Start));
                }
                lastTransformationEndTime = frameTransformations[0].End;
                AnimationFrame newFrame = GenerateFrameFromTransformation(baseFrame, [.. frameTransformations, .. previousTransformations]);
                AnimationFrames.Add(newFrame);
                lastFrame = newFrame;
                previousTransformations.AddRange(frameTransformations);
            }
        }

        public static List<List<Transformation>> OrderAndGroupTransformations(List<Transformation> transformations)
        {
            List<List<Transformation>> result = [];
            // Trie les startTimes pour garantir l'ordre chronologique
            List<TimeSingle> startTimes = [.. transformations.Select(transformation => transformation.Start).Distinct()];
            startTimes.Sort();
            foreach (TimeSingle start in startTimes)
            {
                result.Add([.. transformations.Where(transformation => transformation.Start == start)]);
            }
            return result;
        }

        public static AnimationFrame GenerateFrameFromTransformation(AnimationFrame baseFrame, List<Transformation> transformations)
        {
            List<Vec3> newPositions = baseFrame.VerticesPositions;

            // priority to CustomTransformations
            foreach (Transformation transformation in transformations)
            {
                if (transformation is CustomTransformation)
                {
                    newPositions = transformation.Apply(newPositions);
                }
            }

            foreach (Transformation transformation in transformations)
            {
                if (transformation is not CustomTransformation)
                {
                    newPositions = transformation.Apply(newPositions);
                }
            }

            return new AnimationFrame(newPositions, transformations[0].End);
        }

        public CGameCtnMediaBlockTriangles3D ToTriangle3DMediaTrackerBlock(Vec3 position)
        {
            if (AnimationFrames.Count < 2)
            {
                throw new Exception("error : a BlockTriangles3D must have at least 2 frames");
            }

            // CGameCtnMediaBlockTriangles3D triangle3DBlock = CGameCtnMediaBlockTriangles3D.Create(VerticesColor.ToArray()).ForTMUF().Build();
            CGameCtnMediaBlockTriangles3D triangle3DBlock = new()
            {
                Vertices = [.. VerticesColor],
                Triangles = [.. Triangles]
            };
            triangle3DBlock.Keys = AnimationFrames.ConvertAll(frame => frame.ToMediaTrackerKey(triangle3DBlock, position));

            return triangle3DBlock;
        }
    }
}
