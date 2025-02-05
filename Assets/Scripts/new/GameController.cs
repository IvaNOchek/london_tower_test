using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using DG.Tweening;
using System.Collections;

public class GameController : MonoBehaviour
{
    [SerializeField] private Tower _towerPrefab; // Префаб башни (обязательно назначен в инспекторе)
    [SerializeField] private int _numberOfTowers = 3; // Количество башен по умолчанию
    [SerializeField] private Transform _towersParent; // Родительский объект для башен
    [SerializeField] private float _towerSpacing = 1.2f; // Расстояние между башнями
    [SerializeField] private Material _ghostRingMaterial; // Материал для призрачных колец

    private List<Tower> _towers;
    private GameLevel _currentLevel;
    private Ring _selectedRing;
    private int _moves;
    private float _timer;
    private List<Ring> _ghostRings = new List<Ring>();
    private bool _gameStarted;
    private Color[] _ringColors;

    // Внедрённые зависимости
    [Inject] private IGameRulesService _gameRulesService;
    [Inject] private IMovementService _movementService;
    [Inject] private ILevelCompletionService _levelCompletionService;
    [Inject] private IRecordService _recordService;
    [Inject] private ObjectPool _ringPool;
    [Inject] private InputManager _inputManager;
    [Inject] private MainUIController _mainUIController;
    [Inject] private SolutionDisplay _solutionDisplay;
    [Inject] private Ring.Factory _ringFactory;

    [Inject]
    public void Construct(IGameRulesService gameRulesService, IMovementService movementService,
                          ILevelCompletionService levelCompletionService, IRecordService recordService,
                          ObjectPool ringPool, InputManager inputManager,
                          MainUIController mainUIController, SolutionDisplay solutionDisplay, Ring.Factory ringFactory)
    {
        _gameRulesService = gameRulesService;
        _movementService = movementService;
        _levelCompletionService = levelCompletionService;
        _recordService = recordService;
        _ringPool = ringPool;
        _inputManager = inputManager;
        _mainUIController = mainUIController;
        _solutionDisplay = solutionDisplay;
        _ringFactory = ringFactory;
    }

    private void Awake()
    {
        if (_inputManager == null)
        {
            Debug.LogError("InputManager dependency not resolved!");
        }

        if (_mainUIController == null)
        {
            Debug.LogError("MainUIController dependency not resolved!");
        }

        if (PlayerPrefs.HasKey("NumberOfTowers"))
        {
            _numberOfTowers = PlayerPrefs.GetInt("NumberOfTowers");
            PlayerPrefs.DeleteKey("NumberOfTowers");
        }
    }

    private void Start()
    {
        StartCoroutine(InitAfterDelay());
    }

    private IEnumerator InitAfterDelay()
    {
        yield return new WaitForEndOfFrame();

        // Вычисляем количество колец на уровне (определяет высоту каждой башни)
        int numberOfRings = CalculateNumberOfRings(_numberOfTowers);
        CreateTowers(numberOfRings);
        StartGame();
        PositionCamera();
    }

    // Создание башен с заданной ёмкостью (numberOfRings) – для каждой башни вызывается SetupTower
    private void CreateTowers(int numberOfTowers)
    {
        _towers = new List<Tower>();

        for (int i = 0; i < numberOfTowers; i++)
        {
            Tower tower = Instantiate(_towerPrefab, _towersParent);
            tower.transform.localPosition = new Vector3(i * _towerSpacing, 0, 0);
            int towerCapacity = i + 1; 
            tower.SetupTower(towerCapacity);
            _towers.Add(tower);
        }
    }

    private void StartGame()
    {
        _gameStarted = true;

        // Рассчитываем количество колец (арифметическая прогрессия)
        int numberOfRings = CalculateNumberOfRings(_numberOfTowers);
        int numberOfMoves = CalculateMaxMoves(_numberOfTowers);
        _currentLevel = new GameLevel(numberOfRings, numberOfMoves, _towers.Count - 1);
        _moves = 0;
        _timer = 0;

        // Генерируем цвета колец
        _ringColors = GenerateRingColors(numberOfRings);

        // Создаем кольца и размещаем на первой башне
        for (int i = 0; i < numberOfRings; i++)
        {
            Material ringMat = new Material(Shader.Find("Standard"));
            ringMat.color = _ringColors[i];
            Ring ring = _ringFactory.Create(numberOfRings - i, ringMat, false);
            _towers[0].AddRing(ring);
        }

        _mainUIController.UpdateMoves(_moves);
        _mainUIController.UpdateTimer(_timer);
        _mainUIController.UpdateRecord(_numberOfTowers);
        _solutionDisplay.CreateSolutionDisplay(_currentLevel.NumberOfRings, _ringColors);
    }

    private void PositionCamera()
    {
        if (_towers.Count == 0 || !Camera.main) return;

        float centerX = _towers.Average(t => t.transform.position.x);
        float maxDistX = _towers.Max(t => Mathf.Abs(t.transform.position.x - centerX));
        float cameraDistZ = 1.2f * (maxDistX + 5f);
        float cameraDistY = 2f + 0.5f * Mathf.Clamp(_numberOfTowers - 3, 0, 5);

        Vector3 cameraPos = new Vector3(centerX, cameraDistY, -cameraDistZ);
        Camera.main.transform.DOMove(cameraPos, 1f).SetEase(Ease.OutSine);
        Camera.main.transform.DORotate(new Vector3(20f, 0f, 0f), 1f).SetEase(Ease.OutSine);
        Debug.Log("Камера выставлена");
    }

    private int CalculateNumberOfRings(int numberOfTowers)
    {
        int n = numberOfTowers - 2;
        return (n * (6 + (n - 1) * 1)) / 2 + 3;
    }

    private int CalculateMaxMoves(int numberOfTowers)
    {
        int n = 2 * numberOfTowers - 1;
        return n;
    }

    private Color[] GenerateRingColors(int count)
    {
        Color[] colors = new Color[count];
        for (int i = 0; i < count; i++)
        {
            float hue = (float)i / count;
            colors[i] = Color.HSVToRGB(hue, 1f, 1f);
        }
        return colors;
    }

    private void Update()
    {
        if (_gameStarted)
        {
            _timer += Time.deltaTime;
            _mainUIController.UpdateTimer(_timer);
        }
    }

    public void OnRingClicked(Ring ring)
    {
        if (ring.IsGhost) return;

        if (_selectedRing != null)
        {
            _selectedRing.Deselect();
            ClearGhostRings();

            if (_selectedRing == ring)
            {
                _selectedRing = null;
                return;
            }
        }

        _selectedRing = ring;
        _selectedRing.Select();
        ShowPossibleMoves();
    }

    private void ShowPossibleMoves()
    {
        if (_selectedRing == null) return;

        foreach (Tower tower in _towers)
        {
            if (_gameRulesService.IsValidMove(_selectedRing, tower))
            {
                Ring ghostRing = _ringFactory.Create(_selectedRing.Size, _ghostRingMaterial, true);
                ghostRing.transform.position = tower.GetNextPlaceholderPosition();
                ghostRing.GetComponent<Renderer>().material.color = _selectedRing.GetComponent<Renderer>().material.color;

                _ghostRings.Add(ghostRing);
            }
        }
    }

    public void OnGhostRingClicked(Ring ghostRing)
    {
        if (_selectedRing == null || !ghostRing.IsGhost)
            return;

        _movementService.MoveRing(_selectedRing, ghostRing.CurrentTower);
        _moves++;
        _mainUIController.UpdateMoves(_moves);

        _selectedRing.Deselect();
        _selectedRing = null;

        ClearGhostRings();
        CheckWinCondition();
    }

    private void ClearGhostRings()
    {
        foreach (Ring ghost in _ghostRings)
        {
            ghost.ResetToOriginalMaterial();
            _ringPool.Return(ghost.gameObject);
        }
        _ghostRings.Clear();
    }

    public void CheckWinCondition()
    {
        if (_levelCompletionService.IsLevelComplete(_towers, _currentLevel))
        {
            _gameStarted = false;
            GameResult result = new GameResult(_numberOfTowers, _moves, _timer);
            _recordService.SaveRecord(result, _numberOfTowers);
            _mainUIController.ShowWinMessage();
            _mainUIController.UpdateRecord(_numberOfTowers);
            DOVirtual.DelayedCall(2f, RestartLevel);
        }
    }

    public void RestartLevel()
    {
        _gameStarted = false;
        _moves = 0;
        _timer = 0;
        _selectedRing = null;
        ClearGhostRings();

        foreach (Tower tower in _towers)
        {
            while (tower.Rings.Count > 0)
            {
                Ring ring = tower.RemoveRing();
                ring.ResetToOriginalMaterial();
                _ringPool.Return(ring.gameObject);
            }
        }
        foreach (Tower tower in _towers)
        {
            Destroy(tower.gameObject);
        }
        _towers.Clear();
        StartGame();
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
            _inputManager.OnRingClicked -= OnRingClicked;
    }
}