using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;
using System.Linq;

public class Tower : MonoBehaviour
{
    public int Capacity { get; private set; }
    public List<Ring> Rings { get; private set; } = new List<Ring>();

    private ObjectPool<RingPlaceholder> _placeholderPool;
    public List<RingPlaceholder> RingPlaceholders { get; private set; } = new List<RingPlaceholder>();

    private GameObject _ringPlaceholderPrefab;
    private Material _transparentRingMaterial;

    [Inject]
    public void Construct(GameObject ringPlaceholderPrefab, Material transparentRingMaterial)
    {
        _ringPlaceholderPrefab = ringPlaceholderPrefab;
        _transparentRingMaterial = transparentRingMaterial;
    }

    public void Initialize(int capacity, ObjectPool<RingPlaceholder> placeholderPool)
    {
        Capacity = capacity;
        _placeholderPool = placeholderPool;
        CreatePlaceholders();
    }

    private void CreatePlaceholders()
    {
        float placeholderHeight = 0.1f; // Высота плейсхолдера
        float towerHeight = GetComponent<Renderer>().bounds.size.y; // Высота башни
        float spacing = 0.2f; // Расстояние между плейсхолдерами

        float startY = transform.position.y + towerHeight / 2 + placeholderHeight / 2;

        for (int i = 0; i < Capacity; i++)
        {
            Vector3 position = new Vector3(transform.position.x, startY + (placeholderHeight + spacing) * i, transform.position.z);
            RingPlaceholder placeholder = _placeholderPool.Get();
            placeholder.transform.position = position;
            placeholder.transform.SetParent(transform);
            placeholder.Initialize(this, _transparentRingMaterial);
            RingPlaceholders.Add(placeholder);
        }
    }

    public void DestroyPlaceholders(ObjectPool<RingPlaceholder> placeholderPool)
    {
        foreach (var placeholder in RingPlaceholders)
        {
            placeholderPool.ReturnToPool(placeholder);
        }
        RingPlaceholders.Clear();
    }

    public void PlaceRing(Ring ring)
    {
        Rings.Add(ring);
        ring.SetTower(this);

        RingPlaceholder targetPlaceholder = null;
        foreach (var placeholder in RingPlaceholders)
        {
            if (placeholder.CurrentRing == null)
            {
                targetPlaceholder = placeholder;
                break;
            }
        }

        if (targetPlaceholder != null)
        {
            targetPlaceholder.SetRing(ring);
            ring.transform.DOMove(targetPlaceholder.transform.position, 0.5f).SetEase(Ease.OutSine);
            ring.transform.DORotateQuaternion(targetPlaceholder.transform.rotation, 0.5f).SetEase(Ease.OutSine);
        }
    }

    public void RemoveRing(Ring ring)
    {
        Rings.Remove(ring);
        ring.SetTower(null);
        foreach (var placeholder in RingPlaceholders)
        {
            if (placeholder.CurrentRing == ring)
            {
                placeholder.SetRing(null);
                break;
            }
        }
    }

    public bool CanPlaceRingAt(Ring ring, RingPlaceholder placeholder)
    {
        if (placeholder.ParentTower != this) return false;
        if (placeholder.CurrentRing != null) return false;

        int placeholderIndex = RingPlaceholders.IndexOf(placeholder);

        if (Rings.Count == 0)
        {
            return placeholderIndex == 0;
        }
        else
        {
            if (placeholderIndex == 0) return Rings.LastOrDefault() == null || Rings.LastOrDefault().transform.localScale.x > ring.transform.localScale.x;
            if (placeholderIndex > 0 && RingPlaceholders[placeholderIndex - 1].CurrentRing == null) return false;
            return Rings.LastOrDefault() == null || Rings.LastOrDefault().transform.localScale.x > ring.transform.localScale.x;
        }
    }
}