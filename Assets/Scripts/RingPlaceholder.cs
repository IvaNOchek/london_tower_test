using UnityEngine;
using Zenject;

public class RingPlaceholder : MonoBehaviour
{
    public Tower ParentTower { get; private set; }
    public Ring CurrentRing { get; set; }

    private Material _transparentMaterial;
    private Ring _transparentRing;
    private ObjectPool<Ring> _ringPool;
    public class Pool : MemoryPool<RingPlaceholder> { }

    [Inject]
    public void Construct(ObjectPool<Ring> ringPool)
    {
        _ringPool = ringPool;
    }

    public void Initialize(Tower parentTower, Material material)
    {
        ParentTower = parentTower;
        _transparentMaterial = material;
    }

    public void UpdateVisual(Ring selectedRing)
    {
        if (selectedRing == null || CurrentRing != null)
        {
            HideTransparentRing();
            return;
        }

        if (ParentTower.CanPlaceRingAt(selectedRing, this))
            ShowTransparentRing();
        else
            HideTransparentRing();
    }

    public void ShowTransparentRing()
    {
        if (_transparentRing != null) return;

        _transparentRing = _ringPool.Get();
        _transparentRing.Initialize(_transparentMaterial.color);
        _transparentRing.IsTransparent = true;
        _transparentRing.StartBlinking();
    }

    public void HideTransparentRing()
    {
        if (_transparentRing == null) return;

        _ringPool.ReturnToPool(_transparentRing);
        _transparentRing = null;
    }
}