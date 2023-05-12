using System.Numerics;

namespace Nenuacho.EcsLiteQuadTree.Components
{
    public struct PositionWithNearestEntityComponent
    {
        public Vector2 Position;
        public (int Entity, Vector2 Position, float Distance) NearestEntity;
    }
}