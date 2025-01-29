using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Zenject;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : IGameManager
{
    // Инъекции через Zenject
    private readonly GameObject _towerPrefab;
    private readonly GameObject _colorOrderUIPrefab;
    private readonly Transform _colorOrderUIParent;
    private readonly AudioClip _clickSound;
    private readonly AudioClip _moveSound;
    private readonly AudioClip _winSound;
    private readonly AudioClip _loseSound;
    private readonly AudioSource _audioSource;
    private readonly ObjectPool<Ring> _ringPool;
    private readonly ObjectPool<RingPlaceholder> _placeholderPool;

    // Доменные сущности
    private List<Tower> _towers;
    private List<Ring> _rings;
    private Ring _selectedRing;
    private int _moves;
    private int _maxMoves;
    private bool _isGameOver;
    private List<Color> _originalRingColors;
    private List<Color> _shuffledRingColors;
    private int _numTowers;

    // События
    public event System.Action<int> OnMovesUpdated;
    public event System.Action OnGameWon;
    public event System.Action OnGameLost;

    // Свойства
    public int Moves => _moves;
    public bool IsGameOver => _isGameOver;

    [Inject]
    public GameManager(
        GameObject towerPrefab,
        GameObject colorOrderUIPrefab,
        Transform colorOrderUIParent,
        AudioClip clickSound,
        AudioClip moveSound,
        AudioClip winSound,
        AudioClip loseSound,
        AudioSource audioSource,
        ObjectPool<Ring> ringPool,
        ObjectPool<RingPlaceholder> placeholderPool)
    {
        _towerPrefab = towerPrefab;
        _colorOrderUIPrefab = colorOrderUIPrefab;
        _colorOrderUIParent = colorOrderUIParent;
        _clickSound = clickSound;
        _moveSound = moveSound;
        _winSound = winSound;
        _loseSound = loseSound;
        _audioSource = audioSource;
        _ringPool = ringPool;
        _placeholderPool = placeholderPool;

        _towers = new List<Tower>();
        _rings = new List<Ring>();
        _originalRingColors = new List<Color>();
        _shuffledRingColors = new List<Color>();

        InitializeGame();
    }

    private void InitializeGame()
    {
        _numTowers = PlayerPrefs.GetInt("SelectedTowers", 3);
        CalculateMaxMoves();
        StartNewGame();
        PositionCamera();
    }

    private void CalculateMaxMoves()
    {
        _maxMoves = 2 * _numTowers - 1;
    }

    public void StartNewGame()
    {
        ClearScene();

        _towers = new List<Tower>();
        _rings = new List<Ring>();
        _selectedRing = null;
        _moves = 0;
        _isGameOver = false;

        CreateTowers();
        CreateRings();

        // Обновляем UI через события
        OnMovesUpdated?.Invoke(_maxMoves - _moves);
    }

    private void ClearScene()
    {
        // Возвращаем кольца в пул
        foreach (var ring in _rings)
        {
            _ringPool.ReturnToPool(ring);
        }
        _rings.Clear();

        // Удаляем башни и их плейсхолдеры
        foreach (var tower in _towers)
        {
            tower.DestroyPlaceholders();
            Object.Destroy(tower.gameObject);
        }
        _towers.Clear();

        // Очищаем UI отображения цветов
        foreach (Transform child in _colorOrderUIParent)
        {
            Object.Destroy(child.gameObject);
        }
    }

    private void PositionCamera()
    {
        if (_towers == null || _towers.Count == 0 || Camera.main == null) return;

        float centerX = _towers.Average(t => t.transform.position.x);
        float maxDistanceX = _towers.Max(t => Mathf.Abs(t.transform.position.x - centerX));

        float baseDistanceZ = maxDistanceX + 5f;
        float cameraDistanceZ = 1.2f * baseDistanceZ;

        float baseDistanceY = 2f;
        float additionalDistancePerTower = 0.5f;
        float cameraDistanceY = baseDistanceY + Mathf.Clamp(_numTowers - 3, 0, 5) * additionalDistancePerTower;

        Vector3 cameraPosition = new Vector3(centerX, cameraDistanceY, -cameraDistanceZ);
        Camera.main.transform.DOMove(cameraPosition, 1f).SetEase(Ease.OutSine);
        Camera.main.transform.DORotate(new Vector3(20f, 0f, 0f), 1f).SetEase(Ease.OutSine);
    }

    private void CreateTowers()
    {
        Vector3 baseTowerScale = new Vector3(0.2f, 0.33f, 0.2f);

        for (int i = 0; i < _numTowers; i++)
        {
            int capacity = i + 1;
            Vector3 position = new Vector3(i * 5f, 0, 0);
            GameObject towerObj = Object.Instantiate(_towerPrefab, position, Quaternion.identity);
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
        _originalRingColors.Clear();
        _shuffledRingColors.Clear();

        List<Color> availableColors = GenerateDistinctColors(_numTowers);

        for (int i = 0; i < _numTowers; i++)
        {
            Ring ring = _ringPool.Get();
            ring.Initialize(availableColors[i]);
            _rings.Add(ring);
            _originalRingColors.Add(availableColors[i]);

            List<Tower> availableTowers = _towers.Where(t => t.Rings.Count < t.Capacity).ToList();
            if (availableTowers.Count == 0)
            {
                Debug.LogError("Нет доступных башен для размещения кольца!");
                _ringPool.ReturnToPool(ring);
                continue;
            }

            Tower startTower = availableTowers[Random.Range(0, availableTowers.Count)];
            startTower.PlaceRing(ring);
        }

        _shuffledRingColors = new List<Color>(_originalRingColors);
        Shuffle(_shuffledRingColors);
        GenerateColorOrderUI(_shuffledRingColors);
    }

    private List<Color> GenerateDistinctColors(int count)
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < count; i++)
        {
            colors.Add(Color.HSVToRGB(i * 1.0f / count, 1, 1));
        }
        return colors;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            T value = list[k];
            list[k] = list[i];
            list[i] = value;
        }
    }

    private void GenerateColorOrderUI(List<Color> colorOrder)
    {
        // Оптимизировано: переиспользование UI элементов через пуллинг
        // Для простоты примера предполагаем, что количество цветов невелико
        foreach (var color in colorOrder)
        {
            GameObject imageGO = Object.Instantiate(_colorOrderUIPrefab, _colorOrderUIParent);
            Image image = imageGO.GetComponent<Image>();

            if (image != null)
            {
                image.color = color;
            }
            else
            {
                Debug.LogError("ColorOrderUIPrefab does not have an Image component!");
            }
        }
    }

    public void SelectRing(Ring ring)
    {
        if (_selectedRing != null)
        {
            _selectedRing.Deselect();
            HideAllTransparentRings();
        }

        _selectedRing = ring;

        if (_selectedRing != null)
        {
            _selectedRing.Select();
            ShowPossibleMoves();
        }

        _audioSource.PlayOneShot(_clickSound);
    }

    public void MoveRing(RingPlaceholder targetPlaceholder)
    {
        if (_selectedRing == null || _isGameOver) return;

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

        // Обновляем оставшиеся ходы через событие
        if (_maxMoves > 0)
        {
            OnMovesUpdated?.Invoke(_maxMoves - _moves);
        }

        CheckWinCondition();
        _audioSource.PlayOneShot(_moveSound);
    }

    private void ShowPossibleMoves()
    {
        if (_selectedRing == null) return;

        foreach (var tower in _towers)
        {
            if (tower == _selectedRing.CurrentTower)
            {
                bool foundSelectedRing = false;

                foreach (var placeholder in tower.RingPlaceholders)
                {
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

    private void CheckWinCondition()
    {
        Tower targetTower = _towers.OrderByDescending(t => t.Capacity).First();

        bool ringsOnTargetTower = (targetTower.Rings.Count == _rings.Count);
        bool colorsInOrder = CheckColorOrder(targetTower);

        if (ringsOnTargetTower && colorsInOrder)
        {
            _isGameOver = true;
            OnGameWon?.Invoke();
            SaveResult(_moves);
            _audioSource.PlayOneShot(_winSound);
            DOTween.Sequence()
                .AppendInterval(2f)
                .AppendCallback(() => StartNewGame());
        }
        else if (_maxMoves > 0 && _moves >= _maxMoves)
        {
            _audioSource.PlayOneShot(_loseSound);
            _isGameOver = true;
            OnGameLost?.Invoke();
            SaveResult(_moves);
            DOTween.Sequence()
                .AppendInterval(2f)
                .AppendCallback(() => StartNewGame());
        }
    }

    private bool CheckColorOrder(Tower targetTower)
    {
        if (targetTower.Rings.Count != _numTowers) return false;

        for (int i = 0; i < _numTowers; i++)
        {
            if (targetTower.Rings[i].RingColor != _shuffledRingColors[i])
            {
                return false;
            }
        }
        return true;
    }

    private void SaveResult(int moves)
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

    private void HandleWinAnimation()
    {
        foreach (var ring in _rings)
        {
            ring.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 3);
        }

        _towers.ForEach(tower =>
            tower.transform.DOShakePosition(1f, 0.1f));
    }
}