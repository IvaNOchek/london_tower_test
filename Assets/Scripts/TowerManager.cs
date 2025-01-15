/*



using System.Collections.Generic;
using System.Linq;
using UnityEngine;

есть код который нужно исправить, сейчас не появляются все кольца, только одно на самой маленькой башне, также не работают перемещения. вот суть. Создать сцену на Unity, в которой будет представлена башня с тремя основаниями и несколькими кольцами различного размера.
Реализовать возможность перемещения колец с одной основы на другую с помощью мыши.
Реализовать проверку правильности хода и ограничения на количество ходов для завершения уровня.
Добавить возможность отслеживания и сохранения результатов участников.
напиши все необходимые скрипты.
сделай код болле масштабируемым.
количество колец должно быть в соответствии с башней, например, если мы задаем что башни 3, то они должны быть все разного размера, на одну башню поместится только одно кольцо, на вторую 2 кольца, на третью 3 кольца, и в такой игре будет 3 кольца, спавнятся они случайно. есть префаб башни на которую налезет одно кольцо и префаб кольца, в начале игры должно на расстоянии друг от друга появляться заданное количество башен и кольца надетые на башни.
если 3 башни, то колец тоже 3, они должны быть эквивалентны башням и не располагаться на самой большой башне, также прозрачные кольца должны быть везде на башнях. если мы нажали на кольцо, то оно начнет мерцать, а затем если мы навелись на одно из прозрачных колец, и в эту точку можно поставить обычное кольцо (на этом месте нету колец и под ним есть основа или другое кольцо) то оно отобразится и начнет мерцать тоже, если мы нажмем на это призрачное кольцо, то оно пропадет и на его месте окажется наше изначальное кольцо
когда нажимаешь на кольцо, и наводишь на одну из башен, то показывается возможное местоположение (куда можно переместить) учитывая где находятся текущие кольца, в этот момент при наведении мышкой на возможное место появляется полупрозрачное кольцо, которое мерцает, когда мы нажмем на это кольцо, то выбранное кольцо переместится на мерцающее.
перемещать кольца мы можем только по оси z, также каждое кольцо должно быть случайного цвета при появлении.

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Количество башен")]
    public int NumTowers = 3;

    [Tooltip("Префаб башни")]
    public GameObject TowerPrefab;

    [Tooltip("Префаб кольца")]
    public GameObject RingPrefab;

    [Tooltip("Расстояние между башнями")]
    public float TowerSpacing = 5f;

    [Tooltip("Максимальное количество ходов (0 - без ограничений)")]
    public int MaxMoves = 0;

    [Tooltip("Материал для прозрачного кольца")]
    public Material TransparentRingMaterial;

    [Tooltip("Расстояние между плейсхолдерами (по локальной Z-координате)")]
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
        Vector3 baseTowerScale = new Vector3(0.2f, 0.33f, 0.2f); // Базовый размер для башни вместимостью 1

        for (int i = 0; i < NumTowers; i++)
        {
            int capacity = i + 1;
            Vector3 position = new Vector3(i * TowerSpacing, 0, 0);
            GameObject towerObj = Instantiate(TowerPrefab, position, Quaternion.identity, transform);
            Tower tower = towerObj.GetComponent<Tower>();

            // Масштабируем башню по высоте пропорционально вместимости
            towerObj.transform.localScale = new Vector3(baseTowerScale.x, baseTowerScale.y * capacity, baseTowerScale.z);

            // Выравниваем башню по низу
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
        int totalRings = NumTowers; // Количество колец равно количеству башен
        List<int> ringSizes = Enumerable.Range(1, totalRings).ToList();
        ringSizes.Reverse(); // Размеры колец от большего к меньшему (3, 2, 1 для 3 башен)

        // Находим башню с наименьшей вместимостью (первая башня) для начального размещения колец
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
                Debug.LogError("Не удалось найти место для кольца!");
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
                Collider hitCollider = hit.collider; // Получаем коллайдер, в который попал луч

                if (hitCollider != null) // Проверяем, что вообще во что-то попали
                {
                    Ring clickedRing = hitCollider.GetComponent<Ring>();

                    if (clickedRing != null) // Проверяем, что попали в кольцо
                    {
                        if (clickedRing.CurrentTower != null && clickedRing.CurrentTower.Rings.LastOrDefault() == clickedRing) // Доп. проверка CurrentTower
                        {
                            SelectRing(clickedRing);
                        }
                    }
                    else if (hitCollider.GetComponent<RingPlaceholder>())
                    {
                        // Клик по плейсхолдеру обрабатывается в RingPlaceholder
                    }
                    else
                    {
                        SelectRing(null); // Отмена выбора, если клик вне колец/плейсхолдеров
                    }
                }
                else
                {
                    SelectRing(null); // Клик в пустоту
                }
            }
            else
            {
                SelectRing(null); // Raycast не попал ни во что
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
    public Ring TransparentRing { get; private set; } // Добавляем ссылку на прозрачное кольцо
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
    ///  Устанавливает прозрачное кольцо для плейсхолдера.
    /// </summary>
    /// <param name="ring">Прозрачное кольцо.</param>
    public void SetTransparentRing(Ring ring)
    {
        TransparentRing = ring;
    }

    /// <summary>
    ///  Показывает прозрачное кольцо.
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
    ///  Скрывает прозрачное кольцо.
    /// </summary>
    public void HideTransparentRing()
    {
        if (TransparentRing != null)
        {
            TransparentRing.StopBlinking(); // Останавливаем мерцание
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
    [Tooltip("Вместимость башни")]
    public int Capacity { get; private set; }
    public List<Ring> Rings { get; private set; } = new List<Ring>();

    [Tooltip("Префаб RingPlaceholder")]
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
// Ring.cs (компонент кольца)
using UnityEngine;

public class Ring : MonoBehaviour
{
    public int Size { get; private set; } // Размер кольца (больше число - больше кольцо)
    public Tower CurrentTower { get; private set; } // Текущая башня
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
            float lerp = Mathf.PingPong(_blinkTimer, _blinkTime) / _blinkTime;  // Плавное переключение между 0 и 1
            _renderer.material.color = Color.Lerp(_originalColor, Color.white, lerp); // Мерцание
        }
    }
}*/