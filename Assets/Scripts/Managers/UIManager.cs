using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;

public class UIManager : MonoBehaviour, IUIManager
{
    [SerializeField] private TextMeshProUGUI RecordsText;
    [SerializeField] private CanvasScaler _canvasScaler;
    [SerializeField] private Button _restartButton, _exitButton, _clearRecordsButton;
    [SerializeField] private TextMeshProUGUI _remainingMovesText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _bestMovesRecordText, _bestTimeRecordText;
    [SerializeField] private TextMeshProUGUI _winMessageText, _loseMessageText;
    [SerializeField] private GameObject _menuPanel;

    private IGameManager _gameManager;
    private ISaveManager _saveManager;

    private float _elapsedTime;
    private bool _isGameActive;
    private int _currentTowersCount;

    [Inject]
    public void Construct(IGameManager gameManager, ISaveManager saveManager)
    {
        _gameManager = gameManager;
        _saveManager = saveManager;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        ConfigureCanvasScaler();

        _gameManager.OnMovesUpdated += UpdateRemainingMoves;
        _gameManager.OnGameWon += ShowWinMessage;
        _gameManager.OnGameLost += ShowLoseMessage;

        if (_restartButton) _restartButton.onClick.AddListener(RestartGame);
        if (_exitButton) _exitButton.onClick.AddListener(ExitToMenu);
        if (_clearRecordsButton) _clearRecordsButton.onClick.AddListener(ClearRecords);

        // Узнаём, сколько башен выбрал игрок
        _currentTowersCount = PlayerPrefs.GetInt("SelectedTowers", 3);

        // Запускаем игру
        _isGameActive = true;
        _elapsedTime = 0f;

        UpdateRecordsUI();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    private void ConfigureCanvasScaler()
    {
        // Пример адаптации для PC/мобильных устройств
        if (_canvasScaler)
        {
            _canvasScaler.referenceResolution = new Vector2(
                SystemInfo.deviceType == DeviceType.Handheld ? 1080 : 1920,
                SystemInfo.deviceType == DeviceType.Handheld ? 1920 : 1080
            );
            _canvasScaler.matchWidthOrHeight =
                (SystemInfo.deviceType == DeviceType.Handheld) ? 0.5f : 0f;
        }
    }

    public void Tick(float deltaTime)
    {
        if (!_isGameActive) return;

        _elapsedTime += deltaTime;
        _timerText.text = $"Time: {FormatTime(_elapsedTime)}";
    }

    public void UpdateRemainingMoves(int remainingMoves)
    {
        _remainingMovesText.text = $"Moves Left: {remainingMoves}";
    }

    public void ShowWinMessage()
    {
        ShowMessage(_winMessageText, "Victory!");
        _isGameActive = false;
        UpdateRecordsUI(); // обновим рекорды
    }

    public void ShowLoseMessage()
    {
        ShowMessage(_loseMessageText, "No more moves!");
        _isGameActive = false;
    }

    public void RestartGame()
    {
        _isGameActive = true;
        _elapsedTime = 0f;
        _timerText.text = $"Time: {FormatTime(0f)}";
        HideMessage(_winMessageText);
        HideMessage(_loseMessageText);
        _gameManager.StartNewGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ExitToMenu()
    {
        // Просто загрузим основное меню
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void LoadRecords()
    {
        var data = _saveManager.LoadAllRecords();
        if (data == null || data.AllRecords.Count == 0)
        {
            RecordsText.text = "No Records Yet";
            _bestMovesRecordText.text = "Best Moves: -";
            _bestTimeRecordText.text = "Best Time: -";
            return;
        }

        // Выводим список всех
        string all = string.Join("\n", data.AllRecords
            .OrderBy(r => r.TowerCount)
            .Select(r => $"Towers={r.TowerCount}  Moves={r.BestMoves}  Time={FormatTime(r.BestTime)}"));
        RecordsText.text = all;

        // Рекорд для текущего уровня
        var rec = data.AllRecords.Find(r => r.TowerCount == _currentTowersCount);
        if (rec != null)
        {
            _bestMovesRecordText.text = $"Best Moves: {rec.BestMoves}";
            _bestTimeRecordText.text = $"Best Time: {FormatTime(rec.BestTime)}";
        }
        else
        {
            _bestMovesRecordText.text = "Best Moves: -";
            _bestTimeRecordText.text = "Best Time: -";
        }
    }

    public void UpdateRecordsUI()
    {
        LoadRecords();
    }

    public void ClearRecords()
    {
        _saveManager.ClearRecords();
        UpdateRecordsUI();
    }

    public void ShowMenu() => _menuPanel?.SetActive(true);
    public void HideMenu() => _menuPanel?.SetActive(false);

    private void ShowMessage(TextMeshProUGUI messageText, string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        messageText.transform.DOScale(Vector3.one, 0.5f)
            .From(Vector3.zero)
            .SetEase(Ease.OutBack);
    }

    private void HideMessage(TextMeshProUGUI messageText)
    {
        messageText.transform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InBack)
            .OnComplete(() => messageText.gameObject.SetActive(false));
    }

    private string FormatTime(float t)
    {
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:D2}:{s:D2}";
    }
}