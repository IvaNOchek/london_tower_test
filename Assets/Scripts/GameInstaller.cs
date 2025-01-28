using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public GameObject TowerPrefab;
    public GameObject RingPrefab;
    public Material TransparentRingMaterial;
    public GameObject RingPlaceholderPrefab;
    public GameObject ColorOrderUIPrefab;
    public Transform ColorOrderUIParent;
    public AudioClip ClickSound;
    public AudioClip MoveSound;
    public AudioClip WinSound;
    public AudioClip LoseSound;

    public Button RestartButton;
    public Button ExitButton;
    public Button ClearRecordsButton;
    public TextMeshProUGUI RemainingMovesText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI BestMovesRecordText;
    public TextMeshProUGUI BestTimeRecordText;
    public TextMeshProUGUI WinMessageText;
    public TextMeshProUGUI LoseMessageText;
    public AudioSource AudioSource;

    // Меню
    public GameObject MenuPanel;
    public Button StartButton;
    public Button ExitMenuButton;
    public Button RecordsButton;
    public TextMeshProUGUI RecordsText;

    public int DefaultNumTowers = 3;

    public override void InstallBindings()
    {
        // Сервисы
        Container.Bind<IGameManager>().To<GameManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UIManager>().FromInstance(FindObjectOfType<UIManager>()).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InputHandler>().AsSingle().NonLazy();
        Container.Bind<ISaveManager>().To<SaveManager>().AsSingle();

        // Пулы объектов
        Container.Bind<ObjectPool<Ring>>().AsSingle()
            .WithArguments(RingPrefab, 10, (Transform)null);
        Container.Bind<ObjectPool<RingPlaceholder>>().AsSingle()
            .WithArguments(RingPlaceholderPrefab, 10, (Transform)null);

        // Параметры для инъекции
        Container.BindInstance(TowerPrefab).WhenInjectedInto<GameManager>();
        Container.BindInstance(TransparentRingMaterial).WhenInjectedInto<RingPlaceholder>();
        Container.BindInstance(ColorOrderUIPrefab).WhenInjectedInto<GameManager>();
        Container.BindInstance(ColorOrderUIParent).WhenInjectedInto<GameManager>();
        Container.BindInstance(ClickSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(MoveSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(WinSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(LoseSound).WhenInjectedInto<GameManager>();
        Container.BindInstance(AudioSource).WhenInjectedInto<GameManager>();

        // UI элементы
        Container.BindInstance(RestartButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(ExitButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(ClearRecordsButton).WhenInjectedInto<UIManager>();
        Container.BindInstance(RemainingMovesText).WhenInjectedInto<UIManager>();
        Container.BindInstance(TimerText).WhenInjectedInto<UIManager>();
        Container.BindInstance(BestMovesRecordText).WhenInjectedInto<UIManager>();
        Container.BindInstance(BestTimeRecordText).WhenInjectedInto<UIManager>();
        Container.BindInstance(WinMessageText).WhenInjectedInto<UIManager>();
        Container.BindInstance(LoseMessageText).WhenInjectedInto<UIManager>();

        // Меню
        Container.Bind<IMenuManager>().To<MenuManager>().AsSingle()
            .WithArguments(DefaultNumTowers, RecordsText, MenuPanel, StartButton, ExitMenuButton, RecordsButton, FindObjectOfType<UIManager>());
        Container.BindInstance(MenuPanel).WhenInjectedInto<MenuManager>();
        Container.BindInstance(StartButton).WhenInjectedInto<MenuManager>();
        Container.BindInstance(ExitMenuButton).WhenInjectedInto<MenuManager>();
        Container.BindInstance(RecordsButton).WhenInjectedInto<MenuManager>();
        Container.BindInstance(RecordsText).WhenInjectedInto<MenuManager>();

    }
}

// GameLoop.cs (пустой, так как основная логика в UIManager и InputHandler)
public class GameLoop : MonoBehaviour { }

// GameRecord.cs (структура для рекордов)
public class GameRecord
{
    public int BestMoves;
    public float BestTime;
}