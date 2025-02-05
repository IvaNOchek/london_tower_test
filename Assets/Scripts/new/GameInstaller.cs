using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameObject _ringPrefab;
    [SerializeField] private int _initialRingPoolSize = 30;
    [SerializeField] private Material _ghostMaterial;

    public override void InstallBindings()
    {
        // �������� ��������
        Container.Bind<IGameRulesService>().To<GameRulesService>().AsSingle();
        Container.Bind<IMovementService>().To<MovementService>().AsSingle();
        Container.Bind<ILevelCompletionService>().To<LevelCompletionService>().AsSingle();
        Container.Bind<IRecordService>().To<RecordService>().AsSingle();
        Container.Bind<IGameResultRepository>().To<GameResultRepository>().AsSingle();

        // �������� InputManager ��� ������ ���������� �� ����� GameObject
        Container.Bind<InputManager>().FromNewComponentOnNewGameObject().AsSingle();

        // �������� MainUIController � SolutionDisplay �� �������� �����
        Container.Bind<MainUIController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SolutionDisplay>().FromComponentInHierarchy().AsSingle();

        // �������� SceneLoader
        Container.Bind<SceneLoader>().To<SceneLoader>().AsSingle();

        // ������� ��� Ring � �����������
        Container.BindFactory<int, Material, bool, Ring, Ring.Factory>()
                 .FromComponentInNewPrefab(_ringPrefab);

        // �������� ObjectPool � ��������� ���������� ����� WithArguments
        Container.Bind<ObjectPool>()
                 .FromNewComponentOnNewGameObject()
                 .AsSingle()
                 .WithArguments(_ringPrefab, _initialRingPoolSize)
                 .NonLazy();

        // �������� GameController �� �������� �����
        Container.BindInterfacesAndSelfTo<GameController>()
                 .FromComponentInHierarchy()
                 .AsSingle();

        // �������� ��������� ����������� ������
        Container.BindInstance(_ghostMaterial);

        // ��������� SignalBus, ���� ������������
        SignalBusInstaller.Install(Container);
    }
}