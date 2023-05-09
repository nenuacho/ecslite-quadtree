using System.Numerics;

namespace Nenuacho.EcsLiteQuadTree.Core
{
    public struct QuadBounds
    {
        public Vector2 Center;
        public Vector2 Size;

        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public QuadBounds(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;

            Left = Center.X - Size.X * 0.5f;
            Right = Center.X + Size.X * 0.5f;
            Top = Center.Y + Size.Y * 0.5f;
            Bottom = Center.Y - Size.Y * 0.5f;
        }

        public bool Contains(in Vector2 point)
        {
            var hSizeX = Size.X * 0.5f;
            var hSizeY = Size.Y * 0.5f;
            return point.X > Center.X - hSizeX && point.X < Center.X + hSizeX && point.Y > Center.Y - hSizeY && point.Y < Center.Y + hSizeY;
        }

        public override string ToString()
        {
            return @$"Size: {Size}; Center: {Center}";
        }
    }
}