using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tower : MonoBehaviour
{
    [Tooltip("����������� �����")]
    public int Capacity { get; private set; }
    public List<Ring> Rings { get; private set; } = new List<Ring>();

    [Tooltip("������ RingPlaceholder")]
    public GameObject RingPlaceholderPrefab;

    public List<RingPlaceholder> RingPlaceholders { get; private set; } = new List<RingPlaceholder>();

    public void Initialize(int capacity)
    {
        Capacity = capacity;
        Rings = new List<Ring>();
        RingPlaceholders = new List<RingPlaceholder>();
        CreateRingPlaceholders();
    }

    private void CreateRingPlaceholders()
    {
        if (RingPlaceholderPrefab == null)
        {
            Debug.LogError("RingPlaceholderPrefab is not assigned!");
            return;
        }

        RingPlaceholders.Clear();

        Renderer towerRenderer = GetComponent<Renderer>();
        if (towerRenderer == null)
        {
            Debug.LogError($"{name} Tower Prefab must have a Renderer component!");
            return;
        }

        Vector3 bottomPosition = transform.position;
        bottomPosition.y -= towerRenderer.bounds.extents.y - 0.4f;

        float placeholderSpacing = GameManager.Instance.PlaceholderSpacing;

        for (int i = 0; i < Capacity; i++)
        {
            Vector3 placeholderPosition = bottomPosition;
            placeholderPosition.y += placeholderSpacing * i;

            GameObject placeholderObj = Instantiate(RingPlaceholderPrefab, transform);
            if (placeholderObj == null)
            {
                Debug.LogError($"Failed to instantiate RingPlaceholderPrefab for placeholder {i}!");
                continue;
            }
            placeholderObj.transform.position = placeholderPosition;
            placeholderObj.name = $"{name} Placeholder {i}";

            // ������� ���������� ������
            Quaternion ringRotation = Quaternion.Euler(90, 0, 0);
            GameObject transparentRingObj = Instantiate(GameManager.Instance.RingPrefab, placeholderObj.transform.position, ringRotation, placeholderObj.transform);
            Ring transparentRing = transparentRingObj.GetComponent<Ring>();
            transparentRing.Initialize(Color.clear);
            transparentRing.IsTransparent = true;

            Vector3 desiredLocalScale = new Vector3(24f, 24f, 100f);  // ����������� Z �� 100
            Vector3 parentLocalScale = placeholderObj.transform.lossyScale;

            // ������������ z-��������� � ������ �������� ��������
            desiredLocalScale.z /= parentLocalScale.z;

            transparentRingObj.transform.localScale = desiredLocalScale;


            // ������������� ���������� ��������
            Renderer transparentRenderer = transparentRing.GetComponent<Renderer>();
            if (transparentRenderer != null)
            {
                transparentRenderer.material = GameManager.Instance.TransparentRingMaterial;
            }

            transparentRingObj.SetActive(false); // ���������� �������� ���������� ������

            RingPlaceholder placeholder = placeholderObj.GetComponent<RingPlaceholder>();
            if (placeholder == null)
            {
                Debug.LogError($"Failed to get RingPlaceholder component from instantiated object for placeholder {i}! Object name: {placeholderObj.name}");
                Destroy(placeholderObj);
                continue;
            }
            placeholder.SetTower(this);
            placeholder.SetTransparentRing(transparentRing);
            RingPlaceholders.Add(placeholder);
        }
    }

    public void DestroyPlaceholders()
    {
        for (int i = RingPlaceholders.Count - 1; i >= 0; i--)
        {
            if (RingPlaceholders[i] != null)
            {
                foreach (Transform child in RingPlaceholders[i].transform)
                {
                    Destroy(child.gameObject);
                }

                Destroy(RingPlaceholders[i].gameObject);
            }
        }
        RingPlaceholders.Clear();
    }

    public bool CanPlaceRing(Ring ring)
    {
        if (Rings.Count >= Capacity)
        {
            return false; // ����� ���������
        }

        // ��������, ��� ������ ����� ��������� �� ������� (������ �������� ������)
        if (Rings.Count > 0)
        {
            // �������� ������� ������ �����
            Ring topRing = Rings.Last();

            // ���������� �� ������
            if (ring.GetComponent<Renderer>().bounds.size.y >= topRing.GetComponent<Renderer>().bounds.size.y) return false;
        }

        return GetPlaceholderForNewRing() != null; // ���������, ���� �� ��������� �����������
    }

    public bool CanPlaceRingAt(Ring ring, RingPlaceholder placeholder)
    {
        if (placeholder.HasRing)
            return false;

        int placeholderIndex = RingPlaceholders.IndexOf(placeholder);
        if (placeholderIndex == -1)
            return false;

        if (placeholderIndex == 0)
        {
            // ����� ������ ����������� ������ ��������, ���� �� ����
            return true;
        }
        else
        {
            // ���������, ���� �� ������ ���� � ������ �� ������� ������ �� �������
            Ring belowRing = RingPlaceholders[placeholderIndex - 1].CurrentRing;
            if (belowRing == null)
                return false; // �� ����� ���������� ����, ���� ���� �����

            // ���������� ������� ����� �� ������
            Renderer currentRenderer = ring.GetComponent<Renderer>();
            Renderer belowRenderer = belowRing.GetComponent<Renderer>();
            if (currentRenderer == null || belowRenderer == null)
                return false;

            return currentRenderer.bounds.size.y == belowRenderer.bounds.size.y;
        }
    }

    public RingPlaceholder GetPlaceholderForNewRing()
    {
        return RingPlaceholders.FirstOrDefault(p => !p.HasRing);
    }

    public void PlaceRing(Ring ring)
    {
        RingPlaceholder placeholder = GetPlaceholderForNewRing();
        if (placeholder == null)
        {
            Debug.LogWarning($"{name} PlaceRing() called, but no available placeholder found!");
            return;
        }

        Rings.Add(ring);
        Rings = Rings.OrderBy(r => r.GetComponent<Renderer>().bounds.size.y).ToList(); // ��������� �� �������
        ring.SetTower(this);
        placeholder.SetRing(ring);
    }

    public void RemoveRing(Ring ring)
    {
        if (!Rings.Contains(ring)) return;

        Rings.Remove(ring);

        RingPlaceholder placeholder = RingPlaceholders.FirstOrDefault(p => p.CurrentRing == ring);
        if (placeholder != null)
        {
            placeholder.SetRing(null);
        }
    }
}