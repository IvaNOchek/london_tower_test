using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using DG.Tweening;

public class UIManager : MonoBehaviour, IUIManager
{

    [SerializeField] private TextMeshProUGUI RecordsText;
    [SerializeField] private CanvasScaler _canvasScaler;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _clearRecordsButton;
    [SerializeField] private TextMeshProUGUI _remainingMovesText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _bestMovesRecordText;
    [SerializeField] private TextMeshProUGUI _bestTimeRecordText;
    [SerializeField] private TextMeshProUGUI _winMessageText;
    [SerializeField] private TextMeshProUGUI _loseMessageText;
    [SerializeField] private GameObject _menuPanel;

    private IGameManager _gameManager;
    private ISaveManager _saveManager;
    private float _elapsedTime;
    private bool _isGameActive;

    [Inject]
    public void Construct(IGameManager gameManager, ISaveManager saveManager)
    {
        _gameManager = gameManager;
        _saveManager = saveManager;

        Initialize();
    }

    private void Initialize()
    {
        // Настройка адаптивного UI
        ConfigureCanvasScaler();

        // Подписка на события
        _gameManager.OnMovesUpdated += UpdateRemainingMoves;
        _gameManager.OnGameWon += ShowWinMessage;
        _gameManager.OnGameLost += ShowLoseMessage;

        // Настройка кнопок
        _restartButton.onClick.AddListener(RestartGame);
        _exitButton.onClick.AddListener(ExitGame);
        _clearRecordsButton.onClick.AddListener(ClearRecords);
    }

    private void ConfigureCanvasScaler()
    {
        _canvasScaler.referenceResolution = new Vector2(
            SystemInfo.deviceType == DeviceType.Handheld ? 1080 : 1920,
            SystemInfo.deviceType == DeviceType.Handheld ? 1920 : 1080
        );
        _canvasScaler.matchWidthOrHeight = SystemInfo.deviceType == DeviceType.Handheld ? 0.5f : 0f;
    }

    // Реализация методов IUIManager
    public void Tick(float deltaTime)
    {
        if (!_isGameActive) return;
        _elapsedTime += deltaTime;
        _timerText.text = $"Time: {FormatTime(_elapsedTime)}";
    }

    public void ShowMenu() => _menuPanel.SetActive(true);
    public void HideMenu() => _menuPanel.SetActive(false);

    public void UpdateTimer(float time)
    {
        _timerText.text = $"Time: {FormatTime(time)}";
    }

    public void UpdateRemainingMoves(int remainingMoves)
    {
        _remainingMovesText.text = $"Moves Left: {remainingMoves}";
    }

    public void ShowWinMessage()
    {
        ShowMessage(_winMessageText, "Поздравляем! Вы победили!");
        _isGameActive = false;
        _saveManager.SaveGameResult(_gameManager.Moves, _elapsedTime, true);
        UpdateRecordsUI();
    }

    public void ShowLoseMessage()
    {
        ShowMessage(_loseMessageText, "Игра окончена! Ходы исчерпаны.");
        _isGameActive = false;
    }

    public void RestartGame()
    {
        _isGameActive = true;
        _elapsedTime = 0f;
        UpdateTimer(0f);
        HideMessage(_winMessageText);
        HideMessage(_loseMessageText);
        _gameManager.StartNewGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadRecords()
    {
        var record = _saveManager.LoadBestRecord();
        if (record != null)
        {
            _bestMovesRecordText.text = $"Best Moves: {record.BestMoves}";
            _bestTimeRecordText.text = $"Best Time: {FormatTime(record.BestTime)}";
            RecordsText.text = $"Best Moves: {record.BestMoves}\nBest Time: {FormatTime(record.BestTime)}";
        }
        else
        {
            _bestMovesRecordText.text = "Best Moves: -";
            _bestTimeRecordText.text = "Best Time: -";
            RecordsText.text = "No Records Yet";
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

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    private void ShowMessage(TextMeshProUGUI messageText, string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        messageText.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutBack);
    }

    private void HideMessage(TextMeshProUGUI messageText)
    {
        messageText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
            .OnComplete(() => messageText.gameObject.SetActive(false));
    }
}