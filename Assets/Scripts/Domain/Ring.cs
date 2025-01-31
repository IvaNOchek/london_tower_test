using DG.Tweening;
using UnityEngine;
using Zenject;

/// <summary>
/// Кольцо, которое можно перемещать. Имеет цвет, размер, а также визуальные эффекты при выборе/снятии.
/// </summary>
public class Ring : MonoBehaviour
{
    public class Pool : MemoryPool<Ring> { }

    public Color RingColor { get; private set; }
    public bool IsTransparent { get; set; }
    public float Size => transform.localScale.x;

    private Renderer _renderer;
    private Sequence _blinkSequence;

    public Tower CurrentTower { get; set; }

    public void Initialize(Color color)
    {
        RingColor = color;
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = RingColor;
    }

    public void Select()
    {
        // Анимация увеличения
        transform.DOScale(Vector3.one * 1.2f, 0.25f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        // Анимация возвращения размера
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void StartBlinking()
    {
        if (!IsTransparent) return;

        if (_renderer == null) _renderer = GetComponent<Renderer>();

        _blinkSequence = DOTween.Sequence()
            .Append(_renderer.material.DOFade(0.3f, 0.4f))
            .Append(_renderer.material.DOFade(0.9f, 0.4f))
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopBlinking()
    {
        if (_blinkSequence != null && _blinkSequence.IsActive())
        {
            _blinkSequence.Kill();
            _renderer.material.DOFade(1f, 0.2f);
        }
    }

    public void SetTower(Tower tower)
    {
        CurrentTower = tower;
    }
}