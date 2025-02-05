using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using DG.Tweening;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _movesText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _recordMovesText;
    [SerializeField] private TextMeshProUGUI _recordTimeText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    // ������� ��� ��������� � � ���������� ��������� prefab, � ������� ��� �������� TextMeshProUGUI ���������
    [SerializeField] private GameObject _winMessagePrefab;
    [SerializeField] private GameObject _loseMessagePrefab;

    private GameController _gameController;
    private IRecordService _recordService;
    private SceneLoader _sceneLoader;

    [Inject]
    public void Construct(GameController gameController, IRecordService recordService, SceneLoader sceneLoader)
    {
        _gameController = gameController;
        _recordService = recordService;
        _sceneLoader = sceneLoader;
    }

    private void Start()
    {
        _restartButton.onClick.AddListener(() => _gameController.RestartLevel());
        _menuButton.onClick.AddListener(() => _sceneLoader.LoadMenuScene());
    }

    public void UpdateMoves(int moves)
    {
        _movesText.text = $"Moves: {moves}";
    }

    public void UpdateTimer(float time)
    {
        _timerText.text = $"Time: {time:F2}";
    }

    public void UpdateRecord(int numberOfTowers)
    {
        GameResult record = _recordService.GetRecord(numberOfTowers);
        if (record != null)
        {
            _recordMovesText.text = $"Record Moves: {record.Moves}";
            _recordTimeText.text = $"Record Time: {record.Time:F2}";
        }
        else
        {
            _recordMovesText.text = "Record Moves: N/A";
            _recordTimeText.text = "Record Time: N/A";
        }
    }

    public void ShowWinMessage()
    {
        // ������������� prefab ��������� ��������
        GameObject messageObject = Instantiate(_winMessagePrefab, transform);
        TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.text = "You Win!";

        // ��������� ��������� �� ������ (���� prefab �� �������� �����)
        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        // �������� ��������� (DoTween)
        messageObject.transform.localScale = Vector3.zero;
        messageObject.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        // ������� ��������� ����� 2 �������
        Destroy(messageObject, 2f);
    }

    public void ShowLoseMessage()
    {
        // ������������� prefab ��������� ���������
        GameObject messageObject = Instantiate(_loseMessagePrefab, transform);
        TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.text = "You Lose!";

        // ��������� ��������� �� ������ (���� prefab �� �������� �����)
        RectTransform rectTransform = messageObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        // �������� ���������
        messageObject.transform.localScale = Vector3.zero;
        messageObject.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        // ������� ��������� ����� 2 �������
        Destroy(messageObject, 2f);
    }
}