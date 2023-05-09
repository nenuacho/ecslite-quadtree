using System.Numerics;

namespace Nenuacho.EcsLiteQuadTree.Components
{
    public struct PositionWithNearestEntityComponent
    {
        public Vector2 Position;
        public (Vector2 Position, int Entity) NearestEntity;
    }
}