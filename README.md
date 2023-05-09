# ECSLite Quadtree Systems

Расширение для ecslite, которое содержит реализацию структуры quadtree, адаптированную под ECS, а так же системы для работы с ней.
Не зависит от движка и каких либо сторонних библиотек, кроме ECSLite. 
В работе систем нет аллокаций (аллокации только на стадии кэширования)

# Зависимости
[ecslite](https://github.com/Leopotam/ecslite)

[ecslite-threads](https://github.com/Leopotam/ecslite-threads)

# Установка
## В виде unity модуля
Поддерживается установка в виде unity-модуля через git-ссылку в PackageManager или прямое редактирование `Packages/manifest.json`:
```
"com.nenuacho.ecslite.quadtree-systems": "https://github.com/nenuacho/ecslite-quadtree.git",
```

# Интеграция

Для работы систем необходимо создать сервис QuadTreeService с подходящими параметрами
```c#
var quadSvc = new QuadTreeService(new QuadBounds(Vector2.Zero, new Vector2(1000, 1000)));
```
Для этого сервиса корень quadtree будет расположен в нулевой координате и иметь ширину и высоту 1000. 
Так же в него можно передать максимальное кол-во сущностей на квадрат, после которого произойдет разделение (по умолчанию 3)
```c#
var quadSvc = new QuadTreeService(new QuadBounds(Vector2.Zero, new Vector2(1000, 1000)), 2);
```

На данный момент реализованны 2 системы:
1. QuadTreeBuildSystem - система построения дерева
2. QuadTreeFindNearestSystem - система поиска ближайшей сущности

Система построения дерева должа отрабатывать до системы поиска
```c#
           _systems
                .Add(new QuadTreeBuildSystem(quadSvc))
                .Add(new QuadTreeFindNearestSystem(quadSvc))
                .Init();
```
Эти системы будут обрабатывать все сущности с компонентом PositionWithNearestEntityComponent.

Поскольку система QuadTreeFindNearestSystem является многопоточной, в неё можно передать размер чанка, это будет число сущностей на поток (по умолчанию 300)
```c#
           _systems
                .Add(new QuadTreeBuildSystem(quadSvc))
                .Add(new QuadTreeFindNearestSystem(quadSvc, 200))
                .Init();
```

Если нужен кастомный фильтр, его так же можно передать в контрукторе систем
```c#
           _systems
                .Add(new QuadTreeBuildSystem(quadSvc, myFilter))
                .Add(new QuadTreeFindNearestSystem(quadSvc, 200, myFilter))
                .Init();
```
при этом PositionWithNearestEntityComponent является обязательным компонентом в фильтре

# Использование

1. Создать сущности с компонентом     
```c#
public struct PositionWithNearestEntityComponent
    {
        public Vector2 Position;
        public (Vector2 Position, int Entity) NearestEntity;
    }
```
2. До работы систем из этого расширения актуализировать Position этих компонентов

3. После работы QuadTreeFindNearestSystem, можно использовать найденные данные для дальнейшех задач, например:
```c#
    public class DrawSystem : IEcsRunSystem
    {
        private EcsFilterInject<Inc<PositionWithNearestEntityComponent>> _filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var e in _filter.Value)
            {
                ref var quadNearest = ref _filter.Pools.Inc1.Get(e);
                Debug.DrawLine(quadNearest.Position.ToUnityVec(), quadNearest.NearestEntity.Position.ToUnityVec(), Color.red);
            }
        }
        ...
```
[GIF](https://s11.gifyu.com/images/q3a.gif)

# Примечание

Поскольку операция поиска ближайших сущностей может быть ресурсоемкой, не следует производить поиск каждый кадр, лучше подобрать более длительный интервал. 
А с интервалами пожет [Interval Systems](https://github.com/nenuacho/ecslite-interval-systems)
