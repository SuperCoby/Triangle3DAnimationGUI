using GBX.NET;
using GBX.NET.Engines.Game;
using TmEssentials;

namespace Triangle3DAnimation.Animation
{
    public class AnimationFrame(List<Vec3> verticesPositions, TimeSingle time)
	{
		public List<Vec3> VerticesPositions { get; set; } = verticesPositions;

		public TimeSingle Time { get; set; } = time;

		public CGameCtnMediaBlockTriangles.Key ToMediaTrackerKey(CGameCtnMediaBlockTriangles baseNode, Vec3 offset)
        {
			CGameCtnMediaBlockTriangles.Key key = new(baseNode)
			{
				Time = Time,
				Positions = [.. VerticesPositions.ConvertAll(position => position + offset)]
			};
			return key;
        }
    }
}
