using System.Numerics;
using Nenuacho.EcsLiteQuadTree.Core;

namespace Nenuacho.EcsLiteQuadTree.Services
{
    public sealed class QuadTreeService
    {
        private Quad _root;
        public Quad Root => _root;

        public QuadTreeService(QuadBounds bounds, int maxPoints = 3)
        {
            _root = Quad.CreateRoot(bounds, maxPoints);
        }

        public void AddPosition(in Vector2 position, int e)
        {
            _root.Insert(in position, e);
        }

        public void Reset()
        {
            _root.Reset();
        }

        public (int, Vector2, float) FindNearestEntityTo(in (int, Vector2) position)
        {
            return _root.FindNearestObject(position);
        }

        public int FindNearestEntitiesTo(in (int, Vector2) position, float searchRadius, (int, Vector2, float)[] result)
        {
            return _root.FindNearestObjects(position, searchRadius, result);
        }
    }
}