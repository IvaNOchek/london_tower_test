using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameObject _ringPrefab;
    [SerializeField] private int _initialRingPoolSize = 30;
    [SerializeField] private Material _ghostMaterial;

    public override void InstallBindings()
    {
        // Привязка сервисов
        Container.Bind<IGameRulesService>().To<GameRulesService>().AsSingle();
        Container.Bind<IMovementService>().To<MovementService>().AsSingle();
        Container.Bind<ILevelCompletionService>().To<LevelCompletionService>().AsSingle();
        Container.Bind<IRecordService>().To<RecordService>().AsSingle();
        Container.Bind<IGameResultRepository>().To<GameResultRepository>().AsSingle();

        // Привязка InputManager как нового компонента на новом GameObject
        Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle();

        // Привязка MainUIController и SolutionDisplay из иерархии сцены
        Container.Bind<MainUIController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SolutionDisplay>().FromComponentInHierarchy().AsSingle();

        // Привязка SceneLoader
        Container.Bind<SceneLoader>().To<SceneLoader>().AsSingle();

        // Фабрика для Ring с параметрами
        Container.BindFactory<int, Material, bool, Ring, Ring.Factory>()
                 .FromComponentInNewPrefab(_ringPrefab);

        // Привязка ObjectPool с передачей аргументов через WithArguments
        Container.Bind<ObjectPool>()
                 .FromNewComponentOnNewGameObject()
                 .AsSingle()
                 .WithArguments(_ringPrefab, _initialRingPoolSize)
                 .NonLazy();

        // Привязка GameController из иерархии сцены
        Container.BindInterfacesAndSelfTo<GameController>()
                 .FromComponentInHierarchy()
                 .AsSingle();

        // Привязка материала призрачного кольца
        Container.BindInstance(_ghostMaterial);

        // Установка SignalBus, если используется
        SignalBusInstaller.Install(Container);
    }
}