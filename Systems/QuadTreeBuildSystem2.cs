using Leopotam.EcsLite;
using Nenuacho.EcsLiteQuadTree.Components;
using Nenuacho.EcsLiteQuadTree.Services;

namespace Nenuacho.EcsLiteQuadTree.Systems
{
    public sealed class QuadTreeBuildSystem2 : IEcsInitSystem, IEcsRunSystem
    {
        private readonly QuadTreeService _quadTreeService;
        private EcsFilter _filter;
        private readonly EcsFilter _userFilter;
        private EcsPool<PositionWithNearestEntitiesComponent> _pool;

        public QuadTreeBuildSystem2(QuadTreeService quadTreeService, EcsFilter filter = null)
        {
            _quadTreeService = quadTreeService;
            _userFilter = filter;
        }

        public void Run(IEcsSystems systems)
        {
            _quadTreeService.Reset();
            
            foreach (var e in _filter)
            {
                ref var c = ref _pool.Get(e);
                _quadTreeService.AddPosition(in c.Position, e);
            }
        }

        public void Init(IEcsSystems systems)
        {
            _filter = _userFilter ?? systems.GetWorld().Filter<PositionWithNearestEntitiesComponent>().End();
            _pool = systems.GetWorld().GetPool<PositionWithNearestEntitiesComponent>();
        }
    }
}