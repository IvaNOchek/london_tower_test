using UnityEngine;
using Zenject;

public class RingPlaceholder : MonoBehaviour
{
    public Tower ParentTower { get; private set; }
    public Ring CurrentRing { get; private set; }
    public bool IsOccupied => CurrentRing != null;

    private Material _transparentRingMaterial;
    private Ring _transparentRing;
    private ObjectPool<Ring> _ringPool;

    public void Initialize(Tower parentTower, Material transparentRingMaterial)
    {
        ParentTower = parentTower;
        _transparentRingMaterial = transparentRingMaterial;
    }

    public void SetRing(Ring ring)
    {
        CurrentRing = ring;
    }

    public void ShowTransparentRing()
    {
        if (CurrentRing != null || _transparentRing != null) return;

        _transparentRing = _ringPool.Get();
        _transparentRing.transform.position = transform.position;
        _transparentRing.transform.rotation = transform.rotation;
        _transparentRing.transform.localScale = Vector3.one; // Устанавливаем размер кольца
        _transparentRing.transform.SetParent(transform);
        _transparentRing.Initialize(_transparentRingMaterial.color);
        _transparentRing.IsTransparent = true;
        _transparentRing.StartBlinking();
    }

    public void HideTransparentRing()
    {
        if (_transparentRing != null)
        {
            _transparentRing.StopBlinking();
            _ringPool.ReturnToPool(_transparentRing);
            _transparentRing = null;
        }
    }

    [Inject]
    public void Construct(ObjectPool<Ring> ringPool)
    {
        _ringPool = ringPool;
    }
}