using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using DG.Tweening;

public class UIManager : MonoBehaviour, IUIManager
{
    // Инъекции через Zenject
    private Button _restartButton;
    private Button _exitButton;
    private Button _clearRecordsButton;
    private TextMeshProUGUI _remainingMovesText;
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _bestMovesRecordText;
    private TextMeshProUGUI _bestTimeRecordText;
    private TextMeshProUGUI _winMessageText;
    private TextMeshProUGUI _loseMessageText;
    private IGameManager _gameManager;
    private ISaveManager _saveManager;

    // Внутренние переменные
    private float _elapsedTime = 0f;
    private bool _isGameActive = true;
    private bool _gameStarted = false;

    // Ссылки на объекты меню
    public GameObject MenuPanel;
    public Button StartButton;
    public Button ExitMenuButton;
    public Button RecordsButton;
    public TextMeshProUGUI RecordsText;

    [Inject]
    public void Construct(
        Button restartButton,
        Button exitButton,
        Button clearRecordsButton,
        TextMeshProUGUI remainingMovesText,
        TextMeshProUGUI timerText,
        TextMeshProUGUI bestMovesRecordText,
        TextMeshProUGUI bestTimeRecordText,
        TextMeshProUGUI winMessageText,
        TextMeshProUGUI loseMessageText,
        IGameManager gameManager,
        ISaveManager saveManager)
    {
        _restartButton = restartButton;
        _exitButton = exitButton;
        _clearRecordsButton = clearRecordsButton;
        _remainingMovesText = remainingMovesText;
        _timerText = timerText;
        _bestMovesRecordText = bestMovesRecordText;
        _bestTimeRecordText = bestTimeRecordText;
        _winMessageText = winMessageText;
        _loseMessageText = loseMessageText;
        _gameManager = gameManager;
        _saveManager = saveManager;

        Initialize();
    }

    private void Initialize()
    {
        // Подписываемся на события GameManager
        _gameManager.OnMovesUpdated += UpdateRemainingMoves;
        _gameManager.OnGameWon += ShowWinMessage;
        _gameManager.OnGameLost += ShowLoseMessage;

        // Подписываемся на кнопки
        _restartButton.onClick.AddListener(RestartGame);
        _exitButton.onClick.AddListener(ExitGame);
        _clearRecordsButton.onClick.AddListener(ClearRecords);

        // Инициализация UI
        LoadRecords();
        UpdateRecordsUI();

        // Скрываем сообщения о победе и проигрыше
        HideMessage(_winMessageText);
        HideMessage(_loseMessageText);

        ShowMenu(); // Показываем меню при запуске
    }

    public void ShowMenu()
    {
        MenuPanel.SetActive(true);
        _gameStarted = false;
    }

    public void HideMenu()
    {
        MenuPanel.SetActive(false);
        _gameStarted = true;
        _gameManager.StartNewGame(); // Начинаем новую игру при скрытии меню
    }

    public void Update(float deltaTime)
    {
        if (_isGameActive && _gameStarted)
        {
            _elapsedTime += deltaTime;
            UpdateTimer(_elapsedTime);
        }
    }

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