using UnityEngine;
using Zenject;
using UnityEngine.UI;
using TMPro;

public class GameInstaller : MonoInstaller
{
    [Header("Prefabs")]
    public GameObject TowerPrefab;
    public Ring RingPrefab;
    public RingPlaceholder RingPlaceholderPrefab;
    public GameObject ColorOrderUIPrefab;

    [Header("Materials")]
    public Material TransparentRingMaterial;

    [Header("UI Components")]
    public Transform ColorOrderUIParent;
    public Button RestartButton;
    public AudioClip ClickSound;
    public AudioClip MoveSound;
    public AudioClip WinSound;
    public AudioClip LoseSound;
    public Button ExitButton;
    public Button ClearRecordsButton;
    public TextMeshProUGUI RemainingMovesText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI BestMovesRecordText;
    public TextMeshProUGUI BestTimeRecordText;
    public TextMeshProUGUI WinMessageText;
    public TextMeshProUGUI LoseMessageText;
    public AudioSource AudioSource;
    public GameObject MenuPanel;
    public Button StartButton;
    public Button ExitMenuButton;
    public Button RecordsButton;
    public TextMeshProUGUI RecordsText;
    public int DefaultNumTowers = 3;

    public override void InstallBindings()
    {
        // ���� ��������
        Container.BindMemoryPool<Ring, Ring.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(RingPrefab.gameObject)
            .UnderTransformGroup("RingsPool");

        Container.BindMemoryPool<RingPlaceholder, RingPlaceholder.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(RingPlaceholderPrefab.gameObject)
            .UnderTransformGroup("PlaceholdersPool");

        // �������� ��������
        Container.BindInterfacesAndSelfTo<GameTimer>().AsSingle().NonLazy();
        Container.Bind<IGameManager>().To<GameManager>().AsSingle();
        Container.Bind<IUIManager>().To<UIManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ISaveManager>().To<SaveManager>().AsSingle();
        Container.Bind<InputHandler>().AsSingle().NonLazy();
        Container.Bind<Material>().WithId("TransparentMaterial").FromInstance(TransparentRingMaterial);
        Container.Bind<ObjectPool<RingPlaceholder>>().AsSingle();

        // �������� ���������� GameManager
        Container.BindInstance(TowerPrefab).WhenInjectedInto<GameManager>();
        Container.BindInstance(ColorOrderUIPrefab).WhenInjectedInto<GameManager>();
        Container.BindInstance(ColorOrderUIParent).WhenInjectedInto<GameManager>();
        Container.BindInstance(TransparentRingMaterial).WhenInjectedInto<RingPlaceholder>();
        Container.BindInstance(ClickSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(MoveSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(WinSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(LoseSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(AudioSource).WhenInjectedInto<GameManager>();

        // �������� UI ���������
        Container.BindInstance(RestartButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(ExitButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(ClearRecordsButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(RemainingMovesText).WhenInjectedInto<UIManager>();
        Container.BindInstance(TimerText).WhenInjectedInto<UIManager>();
        Container.BindInstance(BestMovesRecordText).WhenInjectedInto<UIManager>();
        Container.BindInstance(BestTimeRecordText).WhenInjectedInto<UIManager>();
        Container.BindInstance(WinMessageText).WhenInjectedInto<UIManager>();
        Container.BindInstance(LoseMessageText).WhenInjectedInto<UIManager>();
        // �������� ����
        Container.Bind<IMenuManager>().To<MenuManager>().AsSingle()
            .WithArguments(DefaultNumTowers, RecordsText, MenuPanel, StartButton, ExitMenuButton, RecordsButton);
    }
}

public class GameRecord
{
    public int BestMoves;
    public float BestTime;
}