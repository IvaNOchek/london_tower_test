using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public List<Ring> Rings { get; private set; } = new List<Ring>();
    private List<Transform> _placeholders = new List<Transform>();

    [SerializeField] private GameObject _placeholderPrefab;    // ������ ������������
    [SerializeField] private Transform _placeholderContainer; // ������, ������� ������ placeholder'�

    // ����� ����� ��� ��������� ����� � ������ ������� (���������� �����)
    public void SetupTower(int capacity)
    {
        // ������� ���������� (�� ������ �����������������)
        _placeholders.Clear();
        Rings.Clear();

        // ������� ������ ������������ (���� ��� ��������)
        foreach (Transform child in _placeholderContainer)
        {
            // ����� �������� �������� �� ���� ��� �����, ����� �� ������� ������ �������� �������
            if (child.name.Contains("Placeholder_"))
                Destroy(child.gameObject);
        }

        CreatePlaceholders(capacity);

        // �������� ������ ����� �� Y � ����������� �� ���������� �����
        float scaleY = (float)capacity / 3f;
        transform.localScale = new Vector3(0.2f, scaleY, 0.2f);

        // ������������� ����� �� ����� ������
        float baseY = scaleY;
        transform.localPosition = new Vector3(transform.localPosition.x, baseY, transform.localPosition.z);
    }

    private void CreatePlaceholders(int capacity)
    {
        float startY = 0.3f;     // ������� �������� ������� ������������   
        float stepY = 0.55f;     // ��� ����� �������������� � ��������� ��������
        float towerScaleY = transform.localScale.y;  // �������� ������� ����� �� Y

        for (int i = 0; i < capacity; i++)
        {
            GameObject placeholder = Instantiate(_placeholderPrefab, _placeholderContainer);
            placeholder.name = $"Placeholder_{i}";

            // ������������ ���������� �������� �� Y � ������ �������� �����
            float yOffset = startY + (i * stepY * towerScaleY);

            Vector3 localPosition = new Vector3(0f, yOffset, 0f);
            placeholder.transform.localPosition = localPosition;

            _placeholders.Add(placeholder.transform);
        }
    }

    public Vector3 GetNextPlaceholderPosition()
    {
        if (_placeholders.Count == 0)
        {
            Debug.LogError("��� ������������� � �����!");
            return Vector3.zero;
        }

        if (Rings.Count < _placeholders.Count)
        {
            return _placeholders[Rings.Count].position;
        }
        return Vector3.zero;
    }

    public bool CanPlaceRing(Ring ring)
    {
        if (Rings.Count == 0)
        {
            return true;
        }
        return Rings.Last().Size > ring.Size;
    }

    public void AddRing(Ring ring)
    {
        Rings.Add(ring);
        ring.transform.position = GetNextPlaceholderPosition();
        ring.CurrentTower = this;
    }

    public Ring RemoveRing()
    {
        if (Rings.Count == 0)
        {
            return null;
        }

        Ring topRing = Rings.Last();
        Rings.RemoveAt(Rings.Count - 1);
        topRing.CurrentTower = null;
        return topRing;
    }

    public Ring GetTopRing()
    {
        return Rings.Count > 0 ? Rings.Last() : null;
    }
}