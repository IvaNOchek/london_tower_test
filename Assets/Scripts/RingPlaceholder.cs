using UnityEngine;

public class RingPlaceholder : MonoBehaviour
{
    public Tower ParentTower { get; private set; }
    public Ring CurrentRing { get; private set; }
    public Ring TransparentRing { get; private set; }
    public bool HasRing => CurrentRing != null;

    public void SetTower(Tower tower)
    {
        ParentTower = tower;
    }

    public void SetRing(Ring ring)
    {
        CurrentRing = ring;
        if (ring != null)
        {
            // ”станавливаем позицию кольца так, чтобы верхн€€ грань кольца совпадала с верхней гранью плейсхолдера
            ring.transform.position = transform.position;
            ring.transform.rotation = transform.rotation;
        }
    }

    public void SetTransparentRing(Ring ring)
    {
        TransparentRing = ring;
    }

    public void ShowTransparentRing()
    {
        if (TransparentRing != null && !HasRing)
        {
            TransparentRing.gameObject.SetActive(true);
            TransparentRing.StartBlinking();
        }
    }

    public void HideTransparentRing()
    {
        if (TransparentRing != null)
        {
            TransparentRing.StopBlinking();
            TransparentRing.gameObject.SetActive(false);
        }
    }

    public void StartBlinking()
    {
        if (TransparentRing != null)
        {
            TransparentRing.StartBlinking();
        }
    }

    public void StopBlinking()
    {
        if (TransparentRing != null)
        {
            TransparentRing.StopBlinking();
        }
    }
}