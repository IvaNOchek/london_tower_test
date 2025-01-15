using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.UI; // ��������� ��� Image ����������

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("���������� �����")]
    public int NumTowers = 3;
    [Tooltip("������ �����")]
    public GameObject TowerPrefab;
    [Tooltip("������ ������")]
    public GameObject RingPrefab;
    [Tooltip("���������� ����� �������")]
    public float TowerSpacing = 5f;
    [Tooltip("������������ ���������� �����")]
    public int MaxMoves = 0;
    [Tooltip("�������� ��� ����������� ������")]
    public Material TransparentRingMaterial;
    [Tooltip("���������� ����� ��������������")]
    public float PlaceholderSpacing = 1.0f;

    [Header("�����")]
    public AudioClip clickSound;
    public AudioClip moveSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    [Header("UI Elements")] // ��������� ��������� ��� UI ��������� GameManager
    public GameObject ColorOrderUIPrefab; // ������ UI Image ��� ����������� �����
    public Transform ColorOrderUIParent;   // ������, ��� ����� ������������� UI Image ��� ������

    private List<Color> _originalRingColors = new List<Color>(); // ������ ������������ ������ �����
    private List<Color> _shuffledRingColors = new List<Color>(); // ������ ������������ ������ ��� UI
    private int _numRings; // ��������� ���������� ��� ���������� �����

    private int _numTowers;
    private List<Tower> _towers;
    private List<Ring> _rings;
    private Ring _selectedRing;
    private int _moves = 0;
    private bool _gameOver = false;

    public int Moves => _moves;
    public Ring SelectedRing { get { return _selectedRing; } }

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

        // ��������� ���������� ����� �� PlayerPrefs
        _numTowers = PlayerPrefs.GetInt("SelectedTowers", 3);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        CalculateMaxMoves();
        StartCoroutine(InitializeGame());
    }

    private void CalculateMaxMoves()
    {
        MaxMoves = 2 * _numTowers - 1;
    }

    public void StartNewGame()
    {
        ClearScene();

        _towers = new List<Tower>();
        _rings = new List<Ring>();
        _selectedRing = null;
        _moves = 0;
        _gameOver = false;

        CreateTowers();
        CreateRings();

        // ��������� UI
        UIManager.Instance.UpdateRemainingMoves(MaxMoves - _moves);
        UIManager.Instance.LoadRecords();
        UIManager.Instance.UpdateRecordsUI();
    }

    IEnumerator InitializeGame()
    {
        yield return new WaitForSeconds(0.2f);
        StartNewGame();
        PositionCamera(); // �������� ������� ��� ���������������� ������ ����� �������� �����
    }


    private void ClearScene()
    {
        if (_rings != null)
        {
            foreach (var ring in _rings)
            {
                Destroy(ring.gameObject);
            }
            _rings.Clear();
        }

        if (_towers != null)
        {
            foreach (var tower in _towers)
            {
                tower.DestroyPlaceholders();
                Destroy(tower.gameObject);
            }
            _towers.Clear();
        }
        
        // ������� UI ����������� ������
        foreach (Transform child in ColorOrderUIParent)
        {
            Destroy(child.gameObject);
        }

        ClearColorOrderUI();
    }

    private void PositionCamera()
    {
        if (_towers == null || _towers.Count == 0 || Camera.main == null) return;

        // 1. ��������� ����� ���� ����� �� ��� X
        float centerX = _towers.Average(t => t.transform.position.x);

        // 2. ���������� ������������ ���������� �� ������ �� ����� ����� �� ��� X
        float maxDistanceX = 0f;
        foreach (var tower in _towers)
        {
            float distanceX = Mathf.Abs(tower.transform.position.x - centerX);
            if (distanceX > maxDistanceX)
                maxDistanceX = distanceX;
        }

        // 3. ����������� ��������� ������
        float baseDistanceZ = maxDistanceX + TowerSpacing; // ������� ���������� �� Z
        float cameraDistanceZ = 1.2f * baseDistanceZ; // ��������� ���������� �� Z

        float baseDistanceY = 2f; // ����������� ���������� �� Y ��� ��������� ������
        float additionalDistancePerTower = 0.5f; // ������� ������ �� ������ ����� ����� 3
        float cameraDistanceY = baseDistanceY + Mathf.Clamp(_numTowers - 3, 0, 5) * additionalDistancePerTower;

        // 4. ������������� ������ �� ��� X � ��������� ������� �� Y � Z
        Vector3 cameraPosition = new Vector3(centerX, cameraDistanceY, -cameraDistanceZ);
        Camera.main.transform.position = cameraPosition;

        // 5. �������� ���� ������� ������ �� (20�, 0�, 0�)
        Camera.main.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
    }


    private void CreateTowers()
    {
        Vector3 baseTowerScale = new Vector3(0.2f, 0.33f, 0.2f);
        _towers = new List<Tower>(_numTowers); // �������������� ������ � ��������

        for (int i = 0; i < _numTowers; i++)
        {
            int capacity = i + 1;
            Vector3 position = new Vector3(i * TowerSpacing, 0, 0);
            GameObject towerObj = Instantiate(TowerPrefab, position, Quaternion.identity, transform);
            Tower tower = towerObj.GetComponent<Tower>();
            towerObj.transform.localScale = new Vector3(baseTowerScale.x, baseTowerScale.y * capacity, baseTowerScale.z);

            Renderer towerRenderer = towerObj.GetComponent<Renderer>();
            if (towerRenderer != null)
            {
                position.y += towerRenderer.bounds.extents.y;
                towerObj.transform.position = position;
            }

            tower.Initialize(capacity);
            _towers.Add(tower);
        }
    }

    private void CreateRings()
    {
        _numRings = _numTowers;  // ���������� _numTowers ������ NumTowers
        _originalRingColors.Clear(); // ������� ������ ������ ����� ��������� ����� �����
        _shuffledRingColors.Clear();

        System.Random rng = new System.Random();
        List<Color> availableColors = GenerateDistinctColors(_numRings); // ���������� ���������� �����

        for (int i = 0; i < _numRings; i++)
        {
            Quaternion ringRotation = Quaternion.Euler(90, 0, 0);
            GameObject ringObj = Instantiate(RingPrefab, Vector3.zero, ringRotation);
            Ring ring = ringObj.GetComponent<Ring>();
            Color ringColor = availableColors[i]; // ����� ���� �� ������ �� �������
            ring.Initialize(ringColor);
            _rings.Add(ring);
            _originalRingColors.Add(ringColor); // ��������� ������������ ����

            List<Tower> availableTowers = _towers.Where(t => t.Rings.Count < t.Capacity).ToList();
            if (availableTowers.Count == 0)
            {
                Debug.LogError("��� ��������� ����� ��� ���������� ������!");
                Destroy(ringObj);
                continue;
            }

            Tower startTower = availableTowers.Where(t => t.Rings.Count < _numRings).OrderBy(t => rng.Next()).FirstOrDefault();
            if (startTower == null)
            {
                startTower = availableTowers[rng.Next(availableTowers.Count)];
            }

            if (startTower != null)
            {
                startTower.PlaceRing(ring);
            }
            else
            {
                Debug.LogError("�� ������� ����� ��������� ����� ��� ������!");
                Destroy(ringObj);
            }
        }

        _shuffledRingColors = new List<Color>(_originalRingColors); // �������� ������������ �����
        Shuffle(_shuffledRingColors); // ������������ ������������� ������
        GenerateColorOrderUI(_shuffledRingColors); // �������� ����� ��� ��������� UI ����������� ������
    }

    // ����� ��� ��������� ������ ���������� ������
    private List<Color> GenerateDistinctColors(int count)
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < count; i++)
        {
            colors.Add(Color.HSVToRGB(i * 1.0f / count, 1, 1)); // ��������� ����� ���������� ������ HSV
        }
        return colors;
    }

    // ����� ��� ������������� ������ (Fisher-Yates shuffle)
    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void GenerateColorOrderUI(List<Color> colorOrder)
    {
        // ������� ���������� UI �������� ������
        ClearColorOrderUI();

        // �������� ������ ������������� ����������
        RectTransform parentRectTransform = ColorOrderUIParent.GetComponent<RectTransform>();
        Vector2 parentSize = parentRectTransform.rect.size;

        // ������������ ������ ������� UI Image, ����� ��� ���������� ����������� �� ������ ��������
        float imageHeight = parentSize.y / colorOrder.Count;
        float imageWidth = parentSize.x; // ��� ����� ������ ������������� ������, ���� �����

        // ������� UI Image ��� ������� ����� � ������
        for (int i = 0; i < colorOrder.Count; i++)
        {
            GameObject imageGO = Instantiate(ColorOrderUIPrefab, ColorOrderUIParent);
            Image image = imageGO.GetComponent<Image>();
            RectTransform imageRectTransform = imageGO.GetComponent<RectTransform>();

            if (image != null)
            {
                image.color = colorOrder[i]; // ������������� ���� UI Image

                // ������������� ������ � ������� UI Image
                imageRectTransform.anchorMin = new Vector2(0, 1 - (i + 1) * (1f / colorOrder.Count));
                imageRectTransform.anchorMax = new Vector2(1, 1 - i * (1f / colorOrder.Count));
                imageRectTransform.anchoredPosition = Vector2.zero;
                imageRectTransform.sizeDelta = Vector2.zero; // ��������, ��� sizeDelta �� ������ �� ������
            }
            else
            {
                Debug.LogError("ColorOrderUIPrefab does not have an Image component!");
            }
        }
    }

    private void ClearColorOrderUI()
    {
        foreach (Transform child in ColorOrderUIParent)
        {
            Destroy(child.gameObject);
        }
    }


    public void SelectRing(Ring ring)
    {
        if (_selectedRing != null)
        {
            _selectedRing.Deselect();
            HideAllTransparentRings(); // �������� ���������� ������ ��� ������ ���������
        }

        _selectedRing = ring;

        if (_selectedRing != null)
        {
            _selectedRing.Select();
            ShowPossibleMoves();
        }

        audioSource.PlayOneShot(clickSound);
    }

    public void MoveRing(RingPlaceholder targetPlaceholder)
    {
        if (_selectedRing == null) return;

        if (!targetPlaceholder.ParentTower.CanPlaceRingAt(_selectedRing, targetPlaceholder))
        {
            Debug.Log("������������ ���!");
            return;
        }

        Tower oldTower = _selectedRing.CurrentTower;
        oldTower.RemoveRing(_selectedRing);
        targetPlaceholder.ParentTower.PlaceRing(_selectedRing);

        _selectedRing.Deselect();
        HideAllTransparentRings();
        _selectedRing = null;
        _moves++;

        // ��������� ���������� ���� � UI
        if (MaxMoves > 0)
        {
            UIManager.Instance.UpdateRemainingMoves(MaxMoves - _moves);
        }

        CheckWinCondition();
        audioSource.PlayOneShot(moveSound);
    }

    private void ShowPossibleMoves()
    {
        if (_selectedRing == null) return;

        foreach (var tower in _towers)
        {
            // ���� ��� ����� � ��������� �������, �������� ������ ����
            if (tower == _selectedRing.CurrentTower)
            {
                bool foundSelectedRing = false;

                foreach (var placeholder in tower.RingPlaceholders)
                {
                    // ��� ������ ������� placeholder � ������� �������, �������� ��� ��� ���
                    if (placeholder.CurrentRing == _selectedRing)
                    {
                        foundSelectedRing = true;
                    }

                    if (foundSelectedRing)
                    {
                        placeholder.HideTransparentRing();
                    }
                    else if (tower.CanPlaceRingAt(_selectedRing, placeholder))
                    {
                        placeholder.ShowTransparentRing();
                    }
                }
            }
            else
            {
                // ��� ������ ����� ���������� ��� ���������� �����������
                foreach (var placeholder in tower.RingPlaceholders)
                {
                    if (tower.CanPlaceRingAt(_selectedRing, placeholder))
                    {
                        placeholder.ShowTransparentRing();
                    }
                }
            }
        }
    }

    private void HideAllTransparentRings()
    {
        foreach (var tower in _towers)
        {
            foreach (var placeholder in tower.RingPlaceholders)
            {
                placeholder.HideTransparentRing();
            }
        }
    }

    void Update()
    {
        HandleRingSelection(); // ������������ ����� � Update
    }

    void HandleRingSelection()
    {
        if (Input.GetMouseButtonDown(0) && !_gameOver)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Ring clickedRing = hit.collider.GetComponent<Ring>();

                if (clickedRing != null)
                {
                    if (clickedRing.IsTransparent)
                    {
                        // ���������� ��������� ������ �� ��� �������
                        RingPlaceholder targetPlaceholder = clickedRing.GetComponentInParent<RingPlaceholder>();
                        if (targetPlaceholder != null)
                        {
                            MoveRing(targetPlaceholder);
                        }
                    }
                    else
                    {
                        // ������������ ����� �������� ������
                        if (clickedRing.CurrentTower != null && clickedRing.CurrentTower.Rings.LastOrDefault() == clickedRing)
                        {
                            if (_selectedRing == clickedRing)
                            {
                                SelectRing(null); // ������� ���������, ���� �������� �� ���� �� ������
                            }
                            else
                            {
                                SelectRing(clickedRing); // �������� ����� ������
                            }
                        }
                    }
                }
                else
                {
                    // ���� �� ������ �����: ������� ���������
                    SelectRing(null);
                }
            }
            else
            {
                // ���� �� ��������� ��������: ������� ���������
                SelectRing(null);
            }
        }
    }


    private void CheckWinCondition()
    {
        Tower targetTower = _towers.OrderByDescending(t => t.Capacity).First();

        bool ringsOnTargetTower = (targetTower.Rings.Count == _rings.Count);
        bool colorsInOrder = CheckColorOrder(targetTower); // ��������� ������� ������

        if (ringsOnTargetTower && colorsInOrder) // ������, ���� ��� ������� ���������
        {
            _gameOver = true;

            // ���������� ������
            UIManager.Instance.StopTimer();

            // �������� ������� � ���������
            UIManager.Instance.SaveGame();

            // ���������� ��������� � ������
            UIManager.Instance.ShowWinMessage();

            Debug.Log("You Win! Moves: " + _moves);

            audioSource.PlayOneShot(winSound);
            // �������������� ���������� ����� 2 �������
            Invoke(nameof(AutoRestart), 2f);
        }
        else if (MaxMoves > 0 && _moves >= MaxMoves)
        {
            audioSource.PlayOneShot(loseSound);

            _gameOver = true;
            Debug.Log("Game Over! Moves limit reached.");
            UIManager.Instance.ShowLoseMessage(); // �������� ��������� � ���������
            UIManager.Instance.SaveGame(); // ��������� ���������
            Invoke(nameof(AutoRestart), 2f); // �������������� ���������� ����� 2 �������
        }
    }

    // ����� ��� �������� ������� ������ �� ������� �����
    private bool CheckColorOrder(Tower targetTower)
    {
        if (targetTower.Rings.Count != _numRings) return false;

        for (int i = 0; i < _numRings; i++)
        {
            if (targetTower.Rings[i].RingColor != _shuffledRingColors[i])
            {
                return false;
            }
        }
        return true;
    }


    private void AutoRestart()
    {
        UIManager.Instance.RestartGame();
    }

    public void SaveResult(int moves)
    {
        if (PlayerPrefs.HasKey("MinMoves"))
        {
            int savedMin = PlayerPrefs.GetInt("MinMoves");
            if (moves < savedMin)
            {
                PlayerPrefs.SetInt("MinMoves", moves);
            }
        }
        else
        {
            PlayerPrefs.SetInt("MinMoves", moves);
        }
        PlayerPrefs.Save();
    }
}