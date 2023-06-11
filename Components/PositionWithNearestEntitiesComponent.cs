using System.Numerics;

namespace Nenuacho.EcsLiteQuadTree.Components
{
    public struct PositionWithNearestEntitiesComponent
    {
        public Vector2 Position;
        public (int Entity, Vector2 Position, float Distance)[] NearestEntities;
        public int Count;
    }
}