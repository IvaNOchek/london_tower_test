using DG.Tweening;
using UnityEngine;
using Zenject;

public class Ring : MonoBehaviour
{
    public int Size { get; private set; }
    public Tower CurrentTower { get; set; }
    public bool IsSelected { get; private set; }
    public bool IsGhost { get; private set; } // ��������� ���� "������������"

    private Renderer _renderer;
    private Color _originalColor;
    private Tweener _blinkTween;
    private Material _originalMaterial; // ���������, ����� ������ ���������

    [Inject]
    public void Construct(int size, Material material, bool isGhost)
    {
        Setup(size, material, isGhost);
    }

    public void Setup(int size, Material material, bool isGhost)
    {
        Size = size;
        _renderer = GetComponent<Renderer>();
        _originalMaterial = material; // ��������� ������������ ��������
        _renderer.material = _originalMaterial; // �������������
        IsGhost = isGhost;
        _originalColor = _renderer.material.color;

        if (IsGhost)
        {
            MakeTransparent(); //������ ����������
        }
    }
    private void MakeTransparent()
    {
        // ������ rendering mode �� Fade (��� Transparent, � ����������� �� �������)
        _renderer.material.SetFloat("_Mode", 2); // 2 - Fade, 3 - Transparent
        _renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _renderer.material.SetInt("_ZWrite", 0);
        _renderer.material.DisableKeyword("_ALPHATEST_ON");
        _renderer.material.EnableKeyword("_ALPHABLEND_ON");
        _renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        _renderer.material.renderQueue = 3000;
    }

    public void Select()
    {
        IsSelected = true;
        Blink();
    }

    public void Deselect()
    {
        IsSelected = false;
        StopBlinking();
    }

    public void Blink()
    {
        if (IsGhost)
        {
            BlinkGhost();
            return;
        }

        if (_blinkTween != null)
        {
            _blinkTween.Kill();
        }
        _blinkTween = _renderer.material.DOColor(Color.white, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void BlinkGhost()
    {
        if (_blinkTween != null)
        {
            _blinkTween.Kill();
        }
        _blinkTween = _renderer.material.DOFade(0.3f, 0.5f) // ���������� ������������
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopBlinking()
    {
        if (_blinkTween != null)
        {
            _blinkTween.Kill();
            if (IsGhost)
            {
                _renderer.material.color = _originalColor;
                return;

            }
            _renderer.material.color = _originalColor;
        }
    }

    public void MoveTo(Tower targetTower, Vector3 targetPosition)
    {
        CurrentTower = targetTower;

        transform.DOMove(targetPosition, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // �������� �� ���������� (���� �����)
            });
    }

    // ���������� ������������ ��������
    public void ResetToOriginalMaterial()
    {
        _renderer.material = _originalMaterial;
        IsGhost = false;
        StopBlinking();

    }

    public class Factory : PlaceholderFactory<int, Material, bool, Ring> { }

}