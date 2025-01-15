/*



using System.Collections.Generic;
using System.Linq;
using UnityEngine;

���� ��� ������� ����� ���������, ������ �� ���������� ��� ������, ������ ���� �� ����� ��������� �����, ����� �� �������� �����������. ��� ����. ������� ����� �� Unity, � ������� ����� ������������ ����� � ����� ����������� � ����������� �������� ���������� �������.
����������� ����������� ����������� ����� � ����� ������ �� ������ � ������� ����.
����������� �������� ������������ ���� � ����������� �� ���������� ����� ��� ���������� ������.
�������� ����������� ������������ � ���������� ����������� ����������.
������ ��� ����������� �������.
������ ��� ����� ��������������.
���������� ����� ������ ���� � ������������ � ������, ��������, ���� �� ������ ��� ����� 3, �� ��� ������ ���� ��� ������� �������, �� ���� ����� ���������� ������ ���� ������, �� ������ 2 ������, �� ������ 3 ������, � � ����� ���� ����� 3 ������, ��������� ��� ��������. ���� ������ ����� �� ������� ������� ���� ������ � ������ ������, � ������ ���� ������ �� ���������� ���� �� ����� ���������� �������� ���������� ����� � ������ ������� �� �����.
���� 3 �����, �� ����� ���� 3, ��� ������ ���� ������������ ������ � �� ������������� �� ����� ������� �����, ����� ���������� ������ ������ ���� ����� �� ������. ���� �� ������ �� ������, �� ��� ������ �������, � ����� ���� �� �������� �� ���� �� ���������� �����, � � ��� ����� ����� ��������� ������� ������ (�� ���� ����� ���� ����� � ��� ��� ���� ������ ��� ������ ������) �� ��� ����������� � ������ ������� ����, ���� �� ������ �� ��� ���������� ������, �� ��� �������� � �� ��� ����� �������� ���� ����������� ������
����� ��������� �� ������, � �������� �� ���� �� �����, �� ������������ ��������� �������������� (���� ����� �����������) �������� ��� ��������� ������� ������, � ���� ������ ��� ��������� ������ �� ��������� ����� ���������� �������������� ������, ������� �������, ����� �� ������ �� ��� ������, �� ��������� ������ ������������ �� ���������.
���������� ������ �� ����� ������ �� ��� z, ����� ������ ������ ������ ���� ���������� ����� ��� ���������.

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    [Tooltip("������������ ���������� ����� (0 - ��� �����������)")]
    public int MaxMoves = 0;

    [Tooltip("�������� ��� ����������� ������")]
    public Material TransparentRingMaterial;

    [Tooltip("���������� ����� �������������� (�� ��������� Z-����������)")]
    public float PlaceholderSpacing = 1f;

    private List<Tower> _towers = new List<Tower>();
    private List<Ring> _rings = new List<Ring>();
    private Ring _selectedRing;
    private int _moves = 0;
    private bool _gameOver = false;

    public Ring SelectedRing => _selectedRing;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        ClearScene();
        CreateTowers();
        CreateRings();
        _moves = 0;
        _gameOver = false;
    }

    void ClearScene()
    {
        foreach (var ring in _rings)
        {
            Destroy(ring.gameObject);
        }
        _rings.Clear();

        foreach (var tower in _towers)
        {
            tower.DestroyPlaceholders();
            Destroy(tower.gameObject);
        }
        _towers.Clear();
    }

    void CreateTowers()
    {
        Vector3 baseTowerScale = new Vector3(0.2f, 0.33f, 0.2f); // ������� ������ ��� ����� ������������ 1

        for (int i = 0; i < NumTowers; i++)
        {
            int capacity = i + 1;
            Vector3 position = new Vector3(i * TowerSpacing, 0, 0);
            GameObject towerObj = Instantiate(TowerPrefab, position, Quaternion.identity, transform);
            Tower tower = towerObj.GetComponent<Tower>();

            // ������������ ����� �� ������ ��������������� �����������
            towerObj.transform.localScale = new Vector3(baseTowerScale.x, baseTowerScale.y * capacity, baseTowerScale.z);

            // ����������� ����� �� ����
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


    void CreateRings()
    {
        int totalRings = NumTowers; // ���������� ����� ����� ���������� �����
        List<int> ringSizes = Enumerable.Range(1, totalRings).ToList();
        ringSizes.Reverse(); // ������� ����� �� �������� � �������� (3, 2, 1 ��� 3 �����)

        // ������� ����� � ���������� ������������ (������ �����) ��� ���������� ���������� �����
        Tower startTower = _towers.OrderBy(t => t.Capacity).First();

        Debug.Log($"Creating {totalRings} rings."); // Debug log added

        for (int i = 0; i < totalRings; i++)
        {
            GameObject ringObj = Instantiate(RingPrefab);
            Ring ring = ringObj.GetComponent<Ring>();
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            ring.Initialize(ringSizes[i], randomColor);
            _rings.Add(ring);

            Debug.Log($"Created ring of size {ringSizes[i]}. Placing on tower {startTower.name}"); // Debug log added

            RingPlaceholder placeholder = startTower.GetPlaceholderForNewRing();
            if (placeholder != null)
            {
                startTower.PlaceRing(ring);
                Debug.Log($"Ring placed successfully on placeholder {placeholder.name} of tower {startTower.name}"); // Debug log added
            }
            else
            {
                Debug.LogError("�� ������� ����� ����� ��� ������!");
                Destroy(ringObj);
            }
        }
        Debug.Log($"Total rings created: {_rings.Count}"); // Debug log added
    }


    public void SelectRing(Ring ring)
    {
        if (_selectedRing != null) _selectedRing.Deselect();

        _selectedRing = ring;
        if (ring != null) _selectedRing.Select();
    }

    public void MoveRing(RingPlaceholder targetPlaceholder)
    {
        if (_selectedRing == null) return;

        Tower oldTower = _selectedRing.CurrentTower;
        oldTower.RemoveRing(_selectedRing);
        targetPlaceholder.ParentTower.PlaceRing(_selectedRing);

        _selectedRing.Deselect();
        _selectedRing = null;
        _moves++;

        CheckWinCondition();
    }

    void Update()
    {
        HandleRingSelection();
        UpdatePlaceholdersVisuals();
    }

    void UpdatePlaceholdersVisuals()
    {
        foreach (Tower tower in _towers)
        {
            foreach (var placeholder in tower.RingPlaceholders)
            {
                if (_selectedRing != null && tower.CanPlaceRing(_selectedRing) && placeholder == tower.GetPlaceholderForNewRing())
                {
                    placeholder.ShowTransparentRing();
                }
                else
                {
                    placeholder.HideTransparentRing();
                }
            }
        }
    }


    void HandleRingSelection()
    {
        if (Input.GetMouseButtonDown(0) && !_gameOver)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Collider hitCollider = hit.collider; // �������� ���������, � ������� ����� ���

                if (hitCollider != null) // ���������, ��� ������ �� ���-�� ������
                {
                    Ring clickedRing = hitCollider.GetComponent<Ring>();

                    if (clickedRing != null) // ���������, ��� ������ � ������
                    {
                        if (clickedRing.CurrentTower != null && clickedRing.CurrentTower.Rings.LastOrDefault() == clickedRing) // ���. �������� CurrentTower
                        {
                            SelectRing(clickedRing);
                        }
                    }
                    else if (hitCollider.GetComponent<RingPlaceholder>())
                    {
                        // ���� �� ������������ �������������� � RingPlaceholder
                    }
                    else
                    {
                        SelectRing(null); // ������ ������, ���� ���� ��� �����/�������������
                    }
                }
                else
                {
                    SelectRing(null); // ���� � �������
                }
            }
            else
            {
                SelectRing(null); // Raycast �� ����� �� �� ���
            }
        }
    }


    void CheckWinCondition()
    {
        Tower targetTower = _towers.OrderByDescending(t => t.Capacity).First();
        if (targetTower.Rings.Count == _rings.Count)
        {
            _gameOver = true;
            Debug.Log("You Win! Moves: " + _moves);
        }
        else if (MaxMoves > 0 && _moves >= MaxMoves)
        {
            _gameOver = true;
            Debug.Log("Game Over! Moves limit reached.");
        }
    }
}
using UnityEngine;

public class RingPlaceholder : MonoBehaviour
{
    public Tower ParentTower { get; private set; }
    public Ring CurrentRing { get; private set; }
    public Ring TransparentRing { get; private set; } // ��������� ������ �� ���������� ������
    public bool HasRing => CurrentRing != null;

    public void SetTower(Tower tower)
    {
        ParentTower = tower;
    }

    public void SetRing(Ring ring)
    {
        CurrentRing = ring;
        if (ring != null)
        {
            ring.transform.position = transform.position;
            ring.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    ///  ������������� ���������� ������ ��� ������������.
    /// </summary>
    /// <param name="ring">���������� ������.</param>
    public void SetTransparentRing(Ring ring)
    {
        TransparentRing = ring;
    }

    /// <summary>
    ///  ���������� ���������� ������.
    /// </summary>
    public void ShowTransparentRing()
    {
        if (TransparentRing != null)
        {
            TransparentRing.gameObject.SetActive(true);
            TransparentRing.StartBlinking();
        }
    }

    /// <summary>
    ///  �������� ���������� ������.
    /// </summary>
    public void HideTransparentRing()
    {
        if (TransparentRing != null)
        {
            TransparentRing.StopBlinking(); // ������������� ��������
            TransparentRing.gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.SelectedRing != null)
        {
            ParentTower.SetHoveredPlaceholder(this);
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance.SelectedRing != null)
        {
            ParentTower.ClearHoveredPlaceholder();
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.SelectedRing != null && ParentTower.HoveredPlaceholder == this && ParentTower.CanPlaceRing(GameManager.Instance.SelectedRing))
        {
            GameManager.Instance.MoveRing(this);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tower : MonoBehaviour
{
    [Tooltip("����������� �����")]
    public int Capacity { get; private set; }
    public List<Ring> Rings { get; private set; } = new List<Ring>();

    [Tooltip("������ RingPlaceholder")]
    public GameObject RingPlaceholderPrefab;


    public List<RingPlaceholder> RingPlaceholders { get; private set; } = new List<RingPlaceholder>();
    public RingPlaceholder HoveredPlaceholder { get; private set; }

    public void Initialize(int capacity)
    {
        Debug.Log($"{name} Tower Initialize() called with capacity: {capacity}");
        Capacity = capacity;
        CalculateBottomRingPosition();
        CreateRingPlaceholders();
    }

    private void CalculateBottomRingPosition()
    {
        Renderer towerRenderer = GetComponent<Renderer>();
        if (towerRenderer == null)
        {
            Debug.LogError($"{name} Tower Prefab must have a Renderer component!");
            return;
        }

        Vector3 center = towerRenderer.bounds.center;
        GameObject bottomPosObj = new GameObject("BottomRingPosition");
        bottomPosObj.transform.SetParent(transform);
        bottomPosObj.transform.localPosition = transform.InverseTransformPoint(new Vector3(center.x, towerRenderer.bounds.min.y, center.z));
        Debug.Log($"{name} Calculated Bottom Ring Position");
    }

    private void CreateRingPlaceholders()
    {
        Debug.Log($"{name} CreateRingPlaceholders() called");
        if (RingPlaceholderPrefab == null)
        {
            Debug.LogError($"{name} RingPlaceholderPrefab is not assigned!");
            return;
        }
        RingPlaceholders.Clear();
        Debug.Log($"{name} RingPlaceholders list cleared. Current count: {RingPlaceholders.Count}");


        Renderer towerRenderer = GetComponent<Renderer>();
        if (towerRenderer == null)
        {
            Debug.LogError($"{name} Tower Prefab must have a Renderer component!");
            return;
        }

        Vector3 bottomPosition = towerRenderer.bounds.center;
        bottomPosition.y = towerRenderer.bounds.min.y;
        float placeholderSpacing = GameManager.Instance.PlaceholderSpacing;
        Debug.Log($"{name} Placeholder Spacing: {placeholderSpacing}");

        for (int i = 0; i < Capacity; i++)
        {
            Vector3 placeholderPosition = bottomPosition + new Vector3(0, placeholderSpacing * (i + 1), 0);
            GameObject placeholderObj = Instantiate(RingPlaceholderPrefab, transform);

            if (placeholderObj == null)
            {
                Debug.LogError($"{name} Failed to instantiate RingPlaceholderPrefab for placeholder {i}!");
                continue; // Skip to next iteration
            }

            placeholderObj.transform.position = placeholderPosition;
            placeholderObj.name = $"{name} Placeholder {i}";

            GameObject transparentRingObj = Instantiate(GameManager.Instance.RingPrefab);
            Ring transparentRing = transparentRingObj.GetComponent<Ring>();
            transparentRing.Initialize(Capacity - i, GameManager.Instance.TransparentRingMaterial.color);
            transparentRing.GetComponent<Renderer>().material = GameManager.Instance.TransparentRingMaterial;
            transparentRingObj.SetActive(false);
            transparentRingObj.transform.position = placeholderPosition;
            transparentRingObj.transform.SetParent(placeholderObj.transform);

            RingPlaceholder placeholder = placeholderObj.GetComponent<RingPlaceholder>();
            if (placeholder == null)
            {
                Debug.LogError($"{name} Failed to get RingPlaceholder component from instantiated object for placeholder {i}! Object name: {placeholderObj.name}");
                Destroy(placeholderObj);
                continue; // Skip to next iteration
            }


            placeholder.SetTower(this);
            placeholder.SetTransparentRing(transparentRing);
            RingPlaceholders.Add(placeholder);
            Debug.Log($"{name} Created Placeholder {placeholder.name} at position {placeholderPosition}. RingPlaceholders count now: {RingPlaceholders.Count}");
        }
        Debug.Log($"{name} Total Placeholders created: {RingPlaceholders.Count}");
    }


    public void DestroyPlaceholders()
    {
        Debug.Log($"{name} DestroyPlaceholders() called");
        foreach (var placeholder in RingPlaceholders)
        {
            foreach (Transform child in placeholder.transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(placeholder.gameObject);
        }
        RingPlaceholders.Clear();
        Debug.Log($"{name} Placeholders cleared. RingPlaceholders count now: {RingPlaceholders.Count}");
    }

    public bool CanPlaceRing(Ring ring)
    {
        if (Rings.Count >= Capacity) return false;
        if (Rings.Count > 0 && ring.Size >= Rings.Last().Size) return false;
        return true;
    }

    public RingPlaceholder GetPlaceholderForNewRing()
    {
        RingPlaceholder availablePlaceholder = RingPlaceholders.FirstOrDefault(p => !p.HasRing);
        Debug.Log($"{name} GetPlaceholderForNewRing() called. Total Placeholders: {RingPlaceholders.Count}, Available placeholder found: {availablePlaceholder != null}");

        if (RingPlaceholders.Count == 0)
        {
            Debug.LogWarning($"{name} GetPlaceholderForNewRing(): RingPlaceholders list is EMPTY!");
        }
        else
        {
            Debug.Log($"{name} GetPlaceholderForNewRing(): Checking placeholders:");
            foreach (var ph in RingPlaceholders)
            {
                Debug.Log($"- Placeholder {ph.name}, HasRing: {ph.HasRing}");
            }
        }

        return availablePlaceholder;
    }

    public void PlaceRing(Ring ring)
    {
        RingPlaceholder placeholder = GetPlaceholderForNewRing();
        if (placeholder == null)
        {
            Debug.LogWarning($"{name} PlaceRing() called, but no available placeholder found!");
            return;
        }
        Rings.Add(ring);
        Rings = Rings.OrderByDescending(r => r.Size).ToList();
        ring.SetTower(this);
        placeholder.SetRing(ring);
        Debug.Log($"{name} Placed ring size {ring.Size} on placeholder {placeholder.name}. Rings count now: {Rings.Count}");
    }

    public void RemoveRing(Ring ring)
    {
        if (!Rings.Contains(ring)) return;

        Rings.Remove(ring);
        RingPlaceholder placeholder = RingPlaceholders.FirstOrDefault(p => p.CurrentRing == ring);
        if (placeholder != null)
        {
            placeholder.SetRing(null);
        }
        Debug.Log($"{name} Removed ring size {ring.Size}. Rings count now: {Rings.Count}");
    }

    public void SetHoveredPlaceholder(RingPlaceholder placeholder)
    {
        HoveredPlaceholder = placeholder;
    }

    public void ClearHoveredPlaceholder()
    {
        HoveredPlaceholder = null;
    }
}
// Ring.cs (��������� ������)
using UnityEngine;

public class Ring : MonoBehaviour
{
    public int Size { get; private set; } // ������ ������ (������ ����� - ������ ������)
    public Tower CurrentTower { get; private set; } // ������� �����
    public bool IsSelected { get; private set; }
    private Renderer _renderer;
    private Color _originalColor;
    private float _blinkTime = 0.5f;
    private float _blinkTimer;
    private bool _isBlinking = false;

    public void Initialize(int size, Color color)
    {
        Size = size;
        _renderer = GetComponent<Renderer>();
        _originalColor = color;
        _renderer.material.color = color;
    }

    public void SetTower(Tower tower)
    {
        CurrentTower = tower;
    }

    public void Select()
    {
        IsSelected = true;
        StartBlinking();
    }

    public void Deselect()
    {
        IsSelected = false;
        StopBlinking();
        _renderer.material.color = _originalColor;
    }

    public void StartBlinking()
    {
        _isBlinking = true;
        _blinkTimer = 0;
    }
    public void StopBlinking()
    {
        _isBlinking = false;
    }

    void Update()
    {
        if (_isBlinking)
        {
            _blinkTimer += Time.deltaTime;
            float lerp = Mathf.PingPong(_blinkTimer, _blinkTime) / _blinkTime;  // ������� ������������ ����� 0 � 1
            _renderer.material.color = Color.Lerp(_originalColor, Color.white, lerp); // ��������
        }
    }
}*/