using System.Linq;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public Color RingColor { get; private set; } // Добавлено свойство для цвета
    public Tower CurrentTower { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsTransparent { get; set; } = false;

    private Renderer _renderer;
    public Color _originalColor;
    [SerializeField] private float _blinkDuration = 1f;
    private float _blinkTimer;
    private bool _isBlinking = false;

    public void Initialize(Color color)
    {
        RingColor = color; // Устанавливаем цвет кольца
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("Ring Prefab must have a Renderer component!");
        }
        _originalColor = color;
        _renderer.material.color = color;
    }

    public void SetTower(Tower tower)
    {
        CurrentTower = tower;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
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
        _blinkTimer = 0f;
    }

    public void StopBlinking()
    {
        _isBlinking = false;
        _renderer.material.color = _originalColor;
    }

    void Update()
    {
        if (_isBlinking)
        {
            _blinkTimer += Time.deltaTime;
            float lerp = (Mathf.Sin(_blinkTimer * Mathf.PI / _blinkDuration) + 1f) / 4f; // Уменьшен коэффициент для более плавного мерцания
            _renderer.material.color = Color.Lerp(_originalColor, Color.white, lerp);
        }
    }
}