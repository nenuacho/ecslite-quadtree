using System;
using System.Linq;
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
        private (int Entity, Vector2 Position)[] _points;


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
            _points ??= new (int, Vector2)[_maxPoints];
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
                _points[_count] = (e, point);
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
        private static float CalculateSqDistance(in Vector2 p1, in Vector2 p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return (float)(dx * dx + dy * dy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SqDistanceToRectangle(in Vector2 point, in QuadBounds bounds)
        {
            float dx = Max(bounds.Left - point.X, 0f, point.X - bounds.Right);
            float dy = Max(bounds.Bottom - point.Y, 0f, point.Y - bounds.Top);
            var r = dx * dx + dy * dy;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Max(in float a, in float b, in float c)
        {
            if (a > b && a > c) return a;
            return b > c ? b : c;
        }

        public (int Entity, Vector2 Position, float distance) FindNearestObject(in (int, Vector2) target)
        {
            var nearestDistance = float.MaxValue;

            (int Entity, Vector2 Position) nearest = (-1, Vector2.Zero);

            FindNearestObjectRecursive(target, ref nearest, ref nearestDistance);

            return (nearest.Entity, nearest.Position, (float)Math.Sqrt(nearestDistance));
        }


        public int FindNearestObjects(in (int, Vector2) target, float radius, (int, Vector2, float)[] result)
        {
            var nearestDistance = float.MaxValue;

            var sqRadius = radius * radius;

            (int Entity, Vector2 Position) nearest = (-1, Vector2.Zero);

            int cnt = 0;
            FindNearestObjectsRecursive(target, sqRadius, ref nearest, ref nearestDistance, result, ref cnt);

            return cnt;
        }


        private void FindNearestObjectsRecursive(
            in (int Entity, Vector2 Position) target,
            float sqRadius,
            ref (int Entity, Vector2 Position) nearest,
            ref float nearestDistance,
            (int Entity, Vector2 Position, float)[] result,
            ref int count)
        {
            // if (nearestDistance == 0)
            // {
            //     return;
            // }

            for (int i = 0; i < _count; i++)
            {
                ref var point = ref _points[i];
                if (target.Entity == point.Entity)
                {
                    continue;
                }

                var distance = CalculateSqDistance(point.Position, target.Position);

                if (distance < sqRadius)
                {
                    UpdateResult(result, (point.Entity, point.Position, distance), ref count);
                }
            }

            if (IsDivided)
            {
                for (int i = 0; i < 4; i++)
                {
                    ref var subQuad = ref Children[i];
                    if (SqDistanceToRectangle(in target.Position, in subQuad.Bounds) <= nearestDistance)
                    {
                        subQuad.FindNearestObjectsRecursive(in target, sqRadius, ref nearest, ref nearestDistance, result, ref count);
                    }
                }
            }
        }

        private void UpdateResult((int, Vector2, float Distance)[] result, in (int, Vector2, float Distance) point, ref int count)
        {
            if (result.Length > count)
            {
                result[count] = point;
                count++;
                return;
            }

            Array.Sort(result, (x, y) => x.Distance.CompareTo(y.Distance));

            for (int i = 0; i < result.Length; i++)
            {
                ref var other = ref result[i];
                if (point.Distance < other.Distance)
                {
                    result[i] = point;
                    return;
                }
            }
        }


        private void FindNearestObjectRecursive(in (int Entity, Vector2 Position) target, ref (int Entity, Vector2 Position) nearest, ref float nearestDistance)
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

                var distance = CalculateSqDistance(point.Position, target.Position);

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
                    if (SqDistanceToRectangle(in target.Position, in subQuad.Bounds) <= nearestDistance)
                    {
                        subQuad.FindNearestObjectRecursive(in target, ref nearest, ref nearestDistance);
                    }
                }
            }
        }
    }
}