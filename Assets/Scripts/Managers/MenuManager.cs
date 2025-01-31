using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ћогика главного меню: выбор количества башен, выход из игры, показ рекордов
/// </summary>
public class MenuManager : IMenuManager
{
    private readonly TextMeshProUGUI _recordsText;
    private readonly GameObject _menuPanel;
    private readonly Button _startButton;
    private readonly Button _exitButton;
    private readonly Button _recordsButton;
    private readonly IUIManager _uiManager;

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

        // ѕоказываем меню при старте
        _menuPanel.SetActive(true);
    }

    public MenuManager(IUIManager uiManager, GameObject menuPanel)
    {
        _uiManager = uiManager;
        _menuPanel = menuPanel;
    }

    public void ShowMenu() => _menuPanel.SetActive(true);
    public void HideMenu() => _menuPanel.SetActive(false);

    public void StartGameWithTowers(int numTowers)
    {
        // «апоминаем выбор игрока
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
        // ѕри нажатии кнопки "Records" Ч загрузить и показать все рекорды
        _uiManager.LoadRecords();
    }
}