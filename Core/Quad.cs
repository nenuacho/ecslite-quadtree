using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Nenuacho.EcsLiteQuadTree.Core
{
    public struct Quad
    {
        public Quad[] Children;
        public QuadBounds Bounds;
        public bool IsDivided;
        private int _maxPoints;
        private int _count;
        private (Vector2 Position, int Entity)[] _points;


        public static Quad CreateRoot(QuadBounds bounds, int maxPoints)
        {
            var root = new Quad();
            root.Init(bounds, maxPoints);
            return root;
        }

        private void Init(QuadBounds bounds, int maxPoints)
        {
            Bounds = bounds;
            IsDivided = false;

            _maxPoints = maxPoints;
            _count = 0;
            
            Children ??= new Quad[4];
            _points ??= new (Vector2, int)[_maxPoints];
        }

        public override string ToString()
        {
            return @$"Bounds: {Bounds}, IsDivided: {IsDivided}, Count: {_count}";
        }

        private void CreateQuad()
        {
            var size = new Vector2(Bounds.Size.X * 0.5f, Bounds.Size.Y * 0.5f);
            var halfX = size.X * 0.5f;
            var halfY = size.Y * 0.5f;

            Children[0].Init(new QuadBounds(Bounds.Center + new Vector2(-halfX, halfY), size), _maxPoints);
            Children[1].Init(new QuadBounds(Bounds.Center + new Vector2(halfX, halfY), size), _maxPoints);
            Children[2].Init(new QuadBounds(Bounds.Center + new Vector2(halfX, -halfY), size), _maxPoints);
            Children[3].Init(new QuadBounds(Bounds.Center + new Vector2(-halfX, -halfY), size), _maxPoints);
            IsDivided = true;
        }

        public void Insert(in Vector2 point, int e)
        {
            if (!Bounds.Contains(point))
            {
                return;
            }

            if (_count < _maxPoints)
            {
                _points[_count] = (point, e);
                _count++;
            }
            else
            {
                if (!IsDivided)
                {
                    CreateQuad();
                }

                for (int i = 0; i < Children.Length; i++)
                {
                    ref var sq = ref Children[i];
                    sq.Insert(point, e);
                }
            }
        }

        public void Reset()
        {
            _count = 0;
            IsDivided = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateDistance(in Vector2 p1, in Vector2 p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float DistanceToRectangle(in Vector2 point, in QuadBounds bounds)
        {
            float dx = Max(bounds.Left - point.X, 0f, point.X - bounds.Right);
            float dy = Max(bounds.Bottom - point.Y, 0f, point.Y - bounds.Top);
            var r = Math.Sqrt(dx * dx + dy * dy);
            return (float)r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Max(in float a, in float b, in float c)
        {
            if (a > b && a > c) return a;
            return b > c ? b : c;
        }

        public (Vector2 Position, int Entity) FindNearestObject(in (Vector2, int) target)
        {
            var nearestDistance = float.MaxValue;

            (Vector2, int) nearest = (Vector2.Zero, -1);

            FindNearestObjectRecursive(target, ref nearest, ref nearestDistance);

            return nearest;
        }

        private void FindNearestObjectRecursive(in (Vector2 Position, int Entity) target, ref (Vector2 Position, int Entity) nearest, ref float nearestDistance)
        {
            if (nearestDistance == 0)
            {
                return;
            }

            for (int i = 0; i < _count; i++)
            {
                ref var point = ref _points[i];
                if (target.Entity == point.Entity)
                {
                    continue;
                }

                var distance = CalculateDistance(point.Position, target.Position);

                if (distance < nearestDistance)
                {
                    nearest = point;
                    nearestDistance = distance;
                }
            }

            if (IsDivided)
            {
                for (int i = 0; i < 4; i++)
                {
                    ref var subQuad = ref Children[i];
                    if (DistanceToRectangle(in target.Position, in subQuad.Bounds) <= nearestDistance)
                    {
                        subQuad.FindNearestObjectRecursive(in target, ref nearest, ref nearestDistance);
                    }
                }
            }
        }
    }
}