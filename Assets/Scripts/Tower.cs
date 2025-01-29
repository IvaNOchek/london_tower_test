using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Tower : MonoBehaviour
{
    public int Capacity { get; private set; }
    public List<Ring> Rings { get; } = new List<Ring>();
    public List<RingPlaceholder> RingPlaceholders { get; } = new List<RingPlaceholder>();

    private ObjectPool<RingPlaceholder> _placeholderPool;
    private Material _transparentMaterial;

    [Inject]
    public void Construct(ObjectPool<RingPlaceholder> placeholderPool, Material transparentMaterial)
    {
        _placeholderPool = placeholderPool;
        _transparentMaterial = transparentMaterial;
    }

    public void Initialize(int capacity)
    {
        Capacity = capacity;
        CreatePlaceholders();
    }

    private void CreatePlaceholders()
    {
        float heightStep = 1f;
        for (int i = 0; i < Capacity; i++)
        {
            var placeholder = _placeholderPool.Get();
            placeholder.transform.position = transform.position + Vector3.up * i * heightStep;
            placeholder.Initialize(this, _transparentMaterial);
            RingPlaceholders.Add(placeholder);
        }
    }

    public void DestroyPlaceholders()
    {
        foreach (var placeholder in RingPlaceholders)
            _placeholderPool.ReturnToPool(placeholder);
        RingPlaceholders.Clear();
    }

    public bool CanPlaceRingAt(Ring ring, RingPlaceholder placeholder)
    {
        return placeholder.ParentTower == this &&
               (Rings.Count == 0 || Rings[^1].Size > ring.Size);
    }

    public void PlaceRing(Ring ring)
    {
        Rings.Add(ring);
        ring.CurrentTower = this;
    }

    public void RemoveRing(Ring ring)
    {
        Rings.Remove(ring);
        ring.CurrentTower = null;
    }
}