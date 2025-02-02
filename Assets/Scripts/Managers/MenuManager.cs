using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Логика главного меню: выбор количества башен, выход из игры, показ рекордов
/// </summary>
/// 
public class MenuManager : MonoBehaviour, IMenuManager
{
    private readonly TextMeshProUGUI _recordsText;
    private readonly GameObject _menuPanel;
    private readonly Button _startButton;
    private readonly Button _exitButton;
    private readonly Button _recordsButton;
    private readonly IUIManager _uiManager;
    private ISaveManager _saveManager;

    private int _defaultNumTowers;



    public MenuManager(
        int defaultNumTowers,
        TextMeshProUGUI recordsText,
        GameObject menuPanel,
        Button startButton,
        Button exitButton,
        Button recordsButton,
        IUIManager uiManager)
    {
        _defaultNumTowers = defaultNumTowers;
        _recordsText = recordsText;
        _menuPanel = menuPanel;
        _startButton = startButton;
        _exitButton = exitButton;
        _recordsButton = recordsButton;
        _uiManager = uiManager;

        _startButton.onClick.AddListener(() => StartGameWithTowers(_defaultNumTowers));
        _exitButton.onClick.AddListener(ExitGame);
        _recordsButton.onClick.AddListener(DisplayRecords);

        // Показываем меню при старте
        _menuPanel.SetActive(true);
    }

    public MenuManager(TextMeshProUGUI recordsText, ISaveManager saveManager)
    {
        _recordsText = recordsText;
        _saveManager = saveManager;
        LoadAllRecords();
    }

    public void ShowMenu() => _menuPanel.SetActive(true);
    public void HideMenu() => _menuPanel.SetActive(false);

    public void StartGameWithTowers(int numTowers)
    {
        // Запоминаем выбор игрока
        PlayerPrefs.SetInt("SelectedTowers", numTowers);

        HideMenu();
        _uiManager.HideMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void DisplayRecords()
    {
        var records = _saveManager.LoadAllRecords().AllRecords;
        string recordsText = "Рекорды:\n";
        foreach (var record in records.OrderBy(r => r.TowerCount))
        {
            recordsText += $"Башен: {record.TowerCount}\nХоды: {record.BestMoves} Время: {FormatTime(record.BestTime)}\n";
        }
        _recordsText.text = recordsText;
    }

    public void LoadAllRecords()
    {
        var records = _saveManager.LoadAllRecords();
        if (records == null || records.AllRecords.Count == 0)
        {
            _recordsText.text = "No records yet!";
            return;
        }

        string recordsStr = "";
        foreach (var rec in records.AllRecords)
        {
            recordsStr += $"Towers: {rec.TowerCount} | Moves: {rec.BestMoves} | Time: {FormatTime(rec.BestTime)}\n";
        }
        _recordsText.text = recordsStr;
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}