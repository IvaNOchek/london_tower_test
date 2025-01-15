using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.UI; // Добавлено для Image компонента

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
    [Tooltip("Максимальное количество ходов")]
    public int MaxMoves = 0;
    [Tooltip("Материал для прозрачного кольца")]
    public Material TransparentRingMaterial;
    [Tooltip("Расстояние между плейсхолдерами")]
    public float PlaceholderSpacing = 1.0f;

    [Header("Звуки")]
    public AudioClip clickSound;
    public AudioClip moveSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    [Header("UI Elements")] // Добавляем заголовок для UI элементов GameManager
    public GameObject ColorOrderUIPrefab; // Префаб UI Image для отображения цвета
    public Transform ColorOrderUIParent;   // Панель, где будут располагаться UI Image для цветов

    private List<Color> _originalRingColors = new List<Color>(); // Список оригинальных цветов колец
    private List<Color> _shuffledRingColors = new List<Color>(); // Список перемешанных цветов для UI
    private int _numRings; // Добавляем переменную для количества колец

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

        // Загружаем количество башен из PlayerPrefs
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

        // Обновляем UI
        UIManager.Instance.UpdateRemainingMoves(MaxMoves - _moves);
        UIManager.Instance.LoadRecords();
        UIManager.Instance.UpdateRecordsUI();
    }

    IEnumerator InitializeGame()
    {
        yield return new WaitForSeconds(0.2f);
        StartNewGame();
        PositionCamera(); // Вызываем функцию для позиционирования камеры после создания башен
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
        
        // Очищаем UI отображения цветов
        foreach (Transform child in ColorOrderUIParent)
        {
            Destroy(child.gameObject);
        }

        ClearColorOrderUI();
    }

    private void PositionCamera()
    {
        if (_towers == null || _towers.Count == 0 || Camera.main == null) return;

        // 1. Вычисляем центр всех башен по оси X
        float centerX = _towers.Average(t => t.transform.position.x);

        // 2. Определяем максимальное расстояние от центра до любой башни по оси X
        float maxDistanceX = 0f;
        foreach (var tower in _towers)
        {
            float distanceX = Mathf.Abs(tower.transform.position.x - centerX);
            if (distanceX > maxDistanceX)
                maxDistanceX = distanceX;
        }

        // 3. Настраиваем параметры камеры
        float baseDistanceZ = maxDistanceX + TowerSpacing; // Базовое расстояние по Z
        float cameraDistanceZ = 1.2f * baseDistanceZ; // Удвоенное расстояние по Z

        float baseDistanceY = 2f; // Уменьшенное расстояние по Y для понижения камеры
        float additionalDistancePerTower = 0.5f; // Прирост высоты за каждую башню сверх 3
        float cameraDistanceY = baseDistanceY + Mathf.Clamp(_numTowers - 3, 0, 5) * additionalDistancePerTower;

        // 4. Центрирование камеры по оси X и установка позиции по Y и Z
        Vector3 cameraPosition = new Vector3(centerX, cameraDistanceY, -cameraDistanceZ);
        Camera.main.transform.position = cameraPosition;

        // 5. Фиксация угла наклона камеры на (20°, 0°, 0°)
        Camera.main.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
    }


    private void CreateTowers()
    {
        Vector3 baseTowerScale = new Vector3(0.2f, 0.33f, 0.2f);
        _towers = new List<Tower>(_numTowers); // Инициализируем список с размером

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
        _numRings = _numTowers;  // Используем _numTowers вместо NumTowers
        _originalRingColors.Clear(); // Очищаем списки цветов перед созданием новых колец
        _shuffledRingColors.Clear();

        System.Random rng = new System.Random();
        List<Color> availableColors = GenerateDistinctColors(_numRings); // Генерируем уникальные цвета

        for (int i = 0; i < _numRings; i++)
        {
            Quaternion ringRotation = Quaternion.Euler(90, 0, 0);
            GameObject ringObj = Instantiate(RingPrefab, Vector3.zero, ringRotation);
            Ring ring = ringObj.GetComponent<Ring>();
            Color ringColor = availableColors[i]; // Берем цвет из списка по порядку
            ring.Initialize(ringColor);
            _rings.Add(ring);
            _originalRingColors.Add(ringColor); // Сохраняем оригинальный цвет

            List<Tower> availableTowers = _towers.Where(t => t.Rings.Count < t.Capacity).ToList();
            if (availableTowers.Count == 0)
            {
                Debug.LogError("Нет доступных башен для размещения кольца!");
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
                Debug.LogError("Не удалось найти свободное место для кольца!");
                Destroy(ringObj);
            }
        }

        _shuffledRingColors = new List<Color>(_originalRingColors); // Копируем оригинальные цвета
        Shuffle(_shuffledRingColors); // Перемешиваем скопированный список
        GenerateColorOrderUI(_shuffledRingColors); // Вызываем метод для генерации UI отображения цветов
    }

    // Метод для генерации списка уникальных цветов
    private List<Color> GenerateDistinctColors(int count)
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < count; i++)
        {
            colors.Add(Color.HSVToRGB(i * 1.0f / count, 1, 1)); // Генерация более различимых цветов HSV
        }
        return colors;
    }

    // Метод для перемешивания списка (Fisher-Yates shuffle)
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
        // Очищаем предыдущие UI элементы цветов
        ClearColorOrderUI();

        // Получаем размер родительского контейнера
        RectTransform parentRectTransform = ColorOrderUIParent.GetComponent<RectTransform>();
        Vector2 parentSize = parentRectTransform.rect.size;

        // Рассчитываем размер каждого UI Image, чтобы они равномерно размещались по высоте родителя
        float imageHeight = parentSize.y / colorOrder.Count;
        float imageWidth = parentSize.x; // Или можно задать фиксированную ширину, если нужно

        // Создаем UI Image для каждого цвета в списке
        for (int i = 0; i < colorOrder.Count; i++)
        {
            GameObject imageGO = Instantiate(ColorOrderUIPrefab, ColorOrderUIParent);
            Image image = imageGO.GetComponent<Image>();
            RectTransform imageRectTransform = imageGO.GetComponent<RectTransform>();

            if (image != null)
            {
                image.color = colorOrder[i]; // Устанавливаем цвет UI Image

                // Устанавливаем размер и позицию UI Image
                imageRectTransform.anchorMin = new Vector2(0, 1 - (i + 1) * (1f / colorOrder.Count));
                imageRectTransform.anchorMax = new Vector2(1, 1 - i * (1f / colorOrder.Count));
                imageRectTransform.anchoredPosition = Vector2.zero;
                imageRectTransform.sizeDelta = Vector2.zero; // Убедимся, что sizeDelta не влияет на размер
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
            HideAllTransparentRings(); // Скрываем прозрачные кольца при снятии выделения
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
            Debug.Log("Недопустимый ход!");
            return;
        }

        Tower oldTower = _selectedRing.CurrentTower;
        oldTower.RemoveRing(_selectedRing);
        targetPlaceholder.ParentTower.PlaceRing(_selectedRing);

        _selectedRing.Deselect();
        HideAllTransparentRings();
        _selectedRing = null;
        _moves++;

        // Обновляем оставшиеся ходы в UI
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
            // Если это башня с выбранным кольцом, скрываем кольца выше
            if (tower == _selectedRing.CurrentTower)
            {
                bool foundSelectedRing = false;

                foreach (var placeholder in tower.RingPlaceholders)
                {
                    // Как только находим placeholder с текущим кольцом, скрываем все над ним
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
                // Для других башен показываем все допустимые перемещения
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
        HandleRingSelection(); // Обрабатываем клики в Update
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
                        // Перемещаем выбранное кольцо на эту позицию
                        RingPlaceholder targetPlaceholder = clickedRing.GetComponentInParent<RingPlaceholder>();
                        if (targetPlaceholder != null)
                        {
                            MoveRing(targetPlaceholder);
                        }
                    }
                    else
                    {
                        // Обрабатываем выбор обычного кольца
                        if (clickedRing.CurrentTower != null && clickedRing.CurrentTower.Rings.LastOrDefault() == clickedRing)
                        {
                            if (_selectedRing == clickedRing)
                            {
                                SelectRing(null); // Снимаем выделение, если кликнули по тому же кольцу
                            }
                            else
                            {
                                SelectRing(clickedRing); // Выбираем новое кольцо
                            }
                        }
                    }
                }
                else
                {
                    // Клик на пустое место: снимаем выделение
                    SelectRing(null);
                }
            }
            else
            {
                // Клик за пределами объектов: снимаем выделение
                SelectRing(null);
            }
        }
    }


    private void CheckWinCondition()
    {
        Tower targetTower = _towers.OrderByDescending(t => t.Capacity).First();

        bool ringsOnTargetTower = (targetTower.Rings.Count == _rings.Count);
        bool colorsInOrder = CheckColorOrder(targetTower); // Проверяем порядок цветов

        if (ringsOnTargetTower && colorsInOrder) // Победа, если оба условия выполнены
        {
            _gameOver = true;

            // Остановить таймер
            UIManager.Instance.StopTimer();

            // Обновить рекорды и сохранить
            UIManager.Instance.SaveGame();

            // Отобразить сообщение о победе
            UIManager.Instance.ShowWinMessage();

            Debug.Log("You Win! Moves: " + _moves);

            audioSource.PlayOneShot(winSound);
            // Автоматический перезапуск через 2 секунды
            Invoke(nameof(AutoRestart), 2f);
        }
        else if (MaxMoves > 0 && _moves >= MaxMoves)
        {
            audioSource.PlayOneShot(loseSound);

            _gameOver = true;
            Debug.Log("Game Over! Moves limit reached.");
            UIManager.Instance.ShowLoseMessage(); // Показать сообщение о проигрыше
            UIManager.Instance.SaveGame(); // Сохранить результат
            Invoke(nameof(AutoRestart), 2f); // Автоматический перезапуск через 2 секунды
        }
    }

    // Метод для проверки порядка цветов на целевой башне
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