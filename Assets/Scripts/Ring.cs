using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Ring : MonoBehaviour
{
    public Color RingColor { get; private set; }
    public bool IsTransparent { get; set; } = false;

    private Renderer _renderer;
    private Sequence _blinkSequence;

    public Tower CurrentTower { get; private set; }

    public void Initialize(Color color)
    {
        RingColor = color;
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = RingColor;
    }

    public void Select()
    {
        // Визуальное выделение кольца
        transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void Deselect()
    {
        // Снятие выделения
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void StartBlinking()
    {
        if (IsTransparent)
        {
            if (_renderer == null) _renderer = GetComponent<Renderer>();

            _blinkSequence = DOTween.Sequence()
                .Append(_renderer.material.DOFade(0.5f, 0.5f))
                .Append(_renderer.material.DOFade(1f, 0.5f))
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void StopBlinking()
    {
        if (_blinkSequence != null && _blinkSequence.IsActive())
        {
            _blinkSequence.Kill();
            if (_renderer != null)
            {
                _renderer.material.DOFade(1f, 0.2f);
            }
        }
    }

    public void SetTower(Tower tower)
    {
        CurrentTower = tower;
    }
}