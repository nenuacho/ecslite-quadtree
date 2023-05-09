using Leopotam.EcsLite;
using Nenuacho.EcsLiteQuadTree.Components;
using Nenuacho.EcsLiteQuadTree.Services;

namespace Nenuacho.EcsLiteQuadTree.Systems
{
    public sealed class QuadTreeBuildSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly QuadTreeService _quadTreeService;
        private EcsFilter _filter;
        private readonly EcsFilter _userFilter;
        private EcsPool<PositionWithNearestEntityComponent> _pool;

        public QuadTreeBuildSystem(QuadTreeService quadTreeService, EcsFilter filter = null)
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
                _quadTreeService.AddPosition(c.Position, e);
            }
        }

        public void Init(IEcsSystems systems)
        {
            _filter = _userFilter ?? systems.GetWorld().Filter<PositionWithNearestEntityComponent>().End();
            _pool = systems.GetWorld().GetPool<PositionWithNearestEntityComponent>();
        }
    }
}