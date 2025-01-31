using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class GameManager : IGameManager
{
    private readonly GameObject _towerPrefab;
    private readonly GameObject _colorOrderUIPrefab;
    private readonly Transform _colorOrderUIParent;
    private readonly AudioClip _clickSound, _moveSound, _winSound, _loseSound;
    private readonly AudioSource _audioSource;
    private readonly Ring.Pool _ringPool;
    private readonly RingPlaceholder.Pool _placeholderPool;
    private readonly ISaveManager _saveManager;

    private List<Tower> _towers;
    private List<Ring> _rings;
    private Ring _selectedRing;

    private List<Color> _originalRingColors;
    private List<Color> _shuffledRingColors;

    private int _numTowers;
    private int _moves;
    private int _maxMoves;
    private bool _isGameOver;
    private float _startTime;

    public event System.Action<int> OnMovesUpdated;
    public event System.Action OnGameWon;
    public event System.Action OnGameLost;

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
        Ring.Pool ringPool,
        RingPlaceholder.Pool placeholderPool,
        ISaveManager saveManager
    )
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
        _saveManager = saveManager;

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
        // Пример: максимально допустимые ходы (можно задать формулу или фиксированное число)
        _maxMoves = 2 * _numTowers - 1;
    }

    public void StartNewGame()
    {
        ClearScene();

        _moves = 0;
        _isGameOver = false;
        _selectedRing = null;
        _rings.Clear();
        _towers.Clear();

        CreateTowers();
        CreateRings();

        _startTime = Time.time;

        OnMovesUpdated?.Invoke(_maxMoves - _moves);
    }

    private void ClearScene()
    {
        foreach (var ring in _rings)
        {
            if (ring != null) _ringPool.Despawn(ring);
        }

        foreach (var tower in _towers)
        {
            tower?.DestroyPlaceholders();
            if (tower && tower.gameObject) Object.Destroy(tower.gameObject);
        }

        _rings.Clear();
        _towers.Clear();

        // Очищаем ColorOrderUI
        for (int i = _colorOrderUIParent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(_colorOrderUIParent.GetChild(i).gameObject);
        }
    }

    private void PositionCamera()
    {
        if (_towers.Count == 0 || !Camera.main) return;

        float centerX = _towers.Average(t => t.transform.position.x);
        float maxDistX = _towers.Max(t => Mathf.Abs(t.transform.position.x - centerX));
        float cameraDistZ = 1.2f * (maxDistX + 5f);
        float cameraDistY = 2f + 0.5f * Mathf.Clamp(_numTowers - 3, 0, 5);

        Vector3 cameraPos = new Vector3(centerX, cameraDistY, -cameraDistZ);
        Camera.main.transform.DOMove(cameraPos, 1f).SetEase(Ease.OutSine);
        Camera.main.transform.DORotate(new Vector3(20f, 0f, 0f), 1f).SetEase(Ease.OutSine);
    }

    private void CreateTowers()
    {
        for (int i = 0; i < _numTowers; i++)
        {
            Vector3 pos = new Vector3(i * 5f, 0f, 0f);
            var towerObj = Object.Instantiate(_towerPrefab, pos, Quaternion.identity);
            Tower tower = towerObj.GetComponent<Tower>();

            // Пример: башня i+1 "высоты", можно обыграть как нужно
            int capacity = i + 1;
            tower.Initialize(capacity);

            // Поднимаем башню, чтобы она была на земле
            if (towerObj.GetComponent<Renderer>() is Renderer rend)
            {
                Vector3 towerPos = towerObj.transform.position;
                towerPos.y += rend.bounds.extents.y;
                towerObj.transform.position = towerPos;
            }

            _towers.Add(tower);
        }
    }

    private void CreateRings()
    {
        _originalRingColors.Clear();
        _shuffledRingColors.Clear();

        List<Color> colors = GenerateDistinctColors(_numTowers);

        for (int i = 0; i < _numTowers; i++)
        {
            var ring = _ringPool.Spawn();
            ring.Initialize(colors[i]);
            _rings.Add(ring);
            _originalRingColors.Add(colors[i]);

            // Положим кольцо на случайную башню, если есть место
            var freeTowers = _towers.Where(t => t.Rings.Count < t.Capacity).ToList();
            if (freeTowers.Count == 0)
            {
                // Нет места — возвращаем кольцо в пул
                _ringPool.Despawn(ring);
                continue;
            }

            Tower chosen = freeTowers[Random.Range(0, freeTowers.Count)];
            chosen.PlaceRing(ring);
        }

        _shuffledRingColors = new List<Color>(_originalRingColors);
        Shuffle(_shuffledRingColors);
        GenerateColorOrderUI(_shuffledRingColors);
    }

    private List<Color> GenerateDistinctColors(int count)
    {
        var list = new List<Color>(count);
        for (int i = 0; i < count; i++)
        {
            // Распределим по HSV для наглядности
            list.Add(Color.HSVToRGB(i / (float)count, 1f, 1f));
        }
        return list;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int idx = Random.Range(0, i + 1);
            (list[i], list[idx]) = (list[idx], list[i]);
        }
    }

    private void GenerateColorOrderUI(List<Color> colors)
    {
        foreach (var c in colors)
        {
            var go = Object.Instantiate(_colorOrderUIPrefab, _colorOrderUIParent);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            if (img) img.color = c;
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

        // Проверка, можем ли мы туда положить кольцо
        if (!targetPlaceholder.ParentTower.CanPlaceRingAt(_selectedRing, targetPlaceholder))
        {
            Debug.LogWarning("Недопустимый ход!");
            return;
        }

        // Убираем кольцо со старой башни
        _selectedRing.CurrentTower.RemoveRing(_selectedRing);
        // Кладём на новую
        targetPlaceholder.ParentTower.PlaceRing(_selectedRing);

        _moves++;
        _selectedRing.Deselect();
        HideAllTransparentRings();
        _selectedRing = null;

        if (_maxMoves > 0)
        {
            int remaining = _maxMoves - _moves;
            OnMovesUpdated?.Invoke(remaining);
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
                bool foundSelected = false;
                foreach (var ph in tower.RingPlaceholders)
                {
                    if (ph.CurrentRing == _selectedRing) foundSelected = true;
                    if (foundSelected) ph.HideTransparentRing();
                    else if (tower.CanPlaceRingAt(_selectedRing, ph)) ph.ShowTransparentRing();
                }
            }
            else
            {
                foreach (var ph in tower.RingPlaceholders)
                {
                    if (tower.CanPlaceRingAt(_selectedRing, ph))
                        ph.ShowTransparentRing();
                }
            }
        }
    }

    private void HideAllTransparentRings()
    {
        foreach (var tower in _towers)
        {
            foreach (var ph in tower.RingPlaceholders)
            {
                ph.HideTransparentRing();
            }
        }
    }

    private void CheckWinCondition()
    {
        // Предположим, что "самая большая" башня — последняя (или ищем по Capacity)
        Tower biggestTower = _towers.OrderByDescending(t => t.Capacity).First();
        bool allOnBiggest = (biggestTower.Rings.Count == _rings.Count);
        bool correctOrder = CheckColorOrder(biggestTower);

        if (allOnBiggest && correctOrder)
        {
            HandleVictory();
        }
        else if (_maxMoves > 0 && _moves >= _maxMoves)
        {
            HandleDefeat();
        }
    }

    private bool CheckColorOrder(Tower tower)
    {
        if (tower.Rings.Count != _numTowers)
            return false;

        for (int i = 0; i < _numTowers; i++)
        {
            if (tower.Rings[i].RingColor != _shuffledRingColors[i])
                return false;
        }
        return true;
    }

    private void HandleVictory()
    {
        _isGameOver = true;
        OnGameWon?.Invoke();

        float finalTime = Time.time - _startTime;
        _saveManager.SaveGameResult(_numTowers, _moves, finalTime, true);

        _audioSource.PlayOneShot(_winSound);

        // Запускаем DoTween Sequence для ожидания и рестарта
        DOTween.Sequence().AppendInterval(2f).AppendCallback(StartNewGame);
    }

    private void HandleDefeat()
    {
        _isGameOver = true;
        OnGameLost?.Invoke();

        _audioSource.PlayOneShot(_loseSound);

        DOTween.Sequence().AppendInterval(2f).AppendCallback(StartNewGame);
    }
}