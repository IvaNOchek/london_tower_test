using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public Button RestartButton;
    public Button ExitButton;
    public Button ClearRecordsButton;
    public TextMeshProUGUI RemainingMovesText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI BestMovesRecordText;
    public TextMeshProUGUI BestTimeRecordText;

    [Header("Win Message")]
    public TextMeshProUGUI WinMessageText;

    [Header("Lose Message")]
    public TextMeshProUGUI LoseMessageText;

    private float _elapsedTime = 0f;
    private bool _isGameActive = true;

    private GameRecord _gameRecord;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RestartButton.onClick.AddListener(RestartGame);
        ExitButton.onClick.AddListener(ExitGame);
        ClearRecordsButton.onClick.AddListener(ClearRecords);

        if (TimerText == null)
        {
            Debug.LogError("TimerText is not assigned in the Inspector!");
        }

        LoadRecords();
        UpdateRecordsUI();

        WinMessageText.gameObject.SetActive(false);
        LoseMessageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_isGameActive)
        {
            _elapsedTime += Time.deltaTime;
            TimerText.text = $"Time: {FormatTime(_elapsedTime)}";
        }
    }

    public void UpdateRemainingMoves(int remainingMoves)
    {
        RemainingMovesText.text = $"Moves Left: {remainingMoves}";
    }

    public void RestartGame()
    {
        LoseMessageText.gameObject.SetActive(false);
        WinMessageText.gameObject.SetActive(false);
        _elapsedTime = 0f;
        _isGameActive = true;
        GameManager.Instance.StartNewGame();
    }

    public void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            int moves = GameManager.Instance.Moves;
            float time = _elapsedTime;

            LoadRecords();

            if (_gameRecord.BestMoves == 0 || moves < _gameRecord.BestMoves)
            {
                _gameRecord.BestMoves = moves;
            }

            if (_gameRecord.BestTime == 0f || time < _gameRecord.BestTime)
            {
                _gameRecord.BestTime = time;
            }

            SaveRecords();
            UpdateRecordsUI();
        }
    }

    public void ClearRecords()
    {
        _gameRecord = new GameRecord { NumOfTowers = GameManager.Instance.NumTowers };
        SaveRecords();
        UpdateRecordsUI();
    }

    public void ExitGame()
    {
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        if (UIManager.Instance != null)
        {
            Destroy(UIManager.Instance.gameObject);
        }

        SceneManager.LoadScene("Menu");
    }

    public void StopTimer()
    {
        _isGameActive = false;
    }

    public void ShowWinMessage()
    {
        WinMessageText.text = "You Win!";
        WinMessageText.gameObject.SetActive(true);
    }

    public void ShowLoseMessage()
    {
        LoseMessageText.text = "You Lose.";
        LoseMessageText.gameObject.SetActive(true);
    }

    public void UpdateRecordsUI()
    {
        if (BestMovesRecordText != null)
        {
            BestMovesRecordText.text = _gameRecord.BestMoves > 0 ? $"Best Moves: {_gameRecord.BestMoves}" : "Best Moves: -";
        }
        if (BestTimeRecordText != null)
        {
            BestTimeRecordText.text = _gameRecord.BestTime > 0f ? $"Best Time: {FormatTime(_gameRecord.BestTime)}" : "Best Time: -";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    private void SaveRecords()
    {
        // Устанавливаем количество башен перед сохранением
        _gameRecord.NumOfTowers = GameManager.Instance.NumTowers;

        string fileName = $"GameRecords_{_gameRecord.NumOfTowers}.json";
        string json = JsonUtility.ToJson(_gameRecord);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), json);
    }

    public void LoadRecords()
    {
        string fileName = $"GameRecords_{GameManager.Instance.NumTowers}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _gameRecord = JsonUtility.FromJson<GameRecord>(json);
        }
        else
        {
            // Инициализируем новый рекорд с текущим количеством башен
            _gameRecord = new GameRecord { NumOfTowers = GameManager.Instance.NumTowers };
        }
    }
}

[System.Serializable]
public class GameRecord
{
    public int NumOfTowers;
    public int BestMoves;
    public float BestTime;
}