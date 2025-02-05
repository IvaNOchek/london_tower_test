using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button[] _towerCountButtons;
    [SerializeField] private TextMeshProUGUI _recordText;

    [Inject] private IRecordService _recordService;
    [Inject] private ISceneLoader _sceneLoader;

    private void Awake()
    {
        _exitButton.onClick.AddListener(Application.Quit);

        for (int i = 0; i < _towerCountButtons.Length; i++)
        {
            int towerCount = i + 3;
            _towerCountButtons[i].onClick.AddListener(() => StartGame(towerCount));
        }
    }
    private void Start()
    {
        for (int i = 0; i < _towerCountButtons.Length; i++)
        {
            Debug.Log($"Кнопка {i} → {i + 3} башен");
        }

        UpdateRecordText();
    }

    public void StartGame(int towerCount)
    {
        if (_sceneLoader == null)
        {
            Debug.LogError("SceneLoader is not initialized! Check if dependencies are injected correctly.");
            return;
        }

        _sceneLoader.LoadMainScene(towerCount);
    }

    private void UpdateRecordText()
    {
        if (_recordService == null)
        {
            Debug.LogError("RecordService is not initialized!");
            return;
        }

        GameResult[] allRecords = _recordService.LoadRecords();

        if (allRecords == null || allRecords.Length == 0)
        {
            _recordText.text = "No records available.";
            return;
        }

        _recordText.text = "";
        foreach (GameResult record in allRecords)
        {
            _recordText.text += $"Towers: {record.Towers}, Moves: {record.Moves}, Time: {record.Time:F2}\n";
        }
    }
}