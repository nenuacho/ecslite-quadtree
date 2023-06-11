using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;
using Nenuacho.EcsLiteQuadTree.Components;
using Nenuacho.EcsLiteQuadTree.Services;

namespace Nenuacho.EcsLiteQuadTree.Systems
{
    public sealed class QuadTreeFindManyNearestSystem : EcsThreadSystem<QuadTreeFindManyNearestSystem.SearchThread, PositionWithNearestEntitiesComponent>, IEcsInitSystem
    {
        private readonly QuadTreeService _quadTreeService;
        private EcsFilter _filter;
        private readonly int _chunkSize;
        private readonly EcsFilter _userFilter;

        public QuadTreeFindManyNearestSystem(QuadTreeService quadTreeService, int chunkSize = 300, EcsFilter filter = null)
        {
            _quadTreeService = quadTreeService;
            _chunkSize = chunkSize;
            _userFilter = filter;
        }

        public void Init(IEcsSystems systems)
        {
            _filter = _userFilter ?? systems.GetWorld().Filter<PositionWithNearestEntitiesComponent>().End();
        }

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return _chunkSize;
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return _filter;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override void SetData(IEcsSystems systems, ref SearchThread thread)
        {
            thread.QuadTreeService = _quadTreeService;
        }

        public struct SearchThread : IEcsThread<PositionWithNearestEntitiesComponent>
        {
            public QuadTreeService QuadTreeService;
            private int[] _entities;
            private PositionWithNearestEntitiesComponent[] _pool;
            private int[] _indices;

            public void Init(int[] entities, PositionWithNearestEntitiesComponent[] pool, int[] indices)
            {
                _entities = entities;
                _pool = pool;
                _indices = indices;
            }

            public void Execute(int fromIndex, int beforeIndex)
            {
                for (int i = fromIndex; i < beforeIndex; i++)
                {
                    var e = _entities[i];
                    ref var c = ref _pool[_indices[e]];
                    c.Count = QuadTreeService.FindNearestEntitiesTo((e, c.Position), 1000f, c.NearestEntities);
                }
            }
        }
    }
}