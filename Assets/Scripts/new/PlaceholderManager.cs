using System.Collections.Generic;
using UnityEngine;

public class PlaceholderManager : MonoBehaviour
{
    private List<Transform> _placeholders = new List<Transform>();

    [SerializeField] private GameObject _placeholderPrefab;    // ������ ������������

    public void SetupPlaceholders(int capacity, Transform parent)
    {
        _placeholders.Clear();

        float startY = 0.3f;     // ������� �������� ������� ������������   
        float stepY = 0.55f;     // ��� ����� �������������� � ��������� ��������
        float towerScaleY = parent.localScale.y;  // �������� ������� ����� �� Y

        for (int i = 0; i < capacity; i++)
        {
            GameObject placeholder = Instantiate(_placeholderPrefab, parent);
            placeholder.name = $"Placeholder_{i}";

            // ������������ ���������� �������� �� Y � ������ �������� �����
            float yOffset = startY + (i * stepY * towerScaleY);

            Vector3 localPosition = new Vector3(0f, yOffset, 0f);
            placeholder.transform.localPosition = localPosition;

            _placeholders.Add(placeholder.transform);
        }
    }

    public Vector3 GetPlaceholderPosition(int index)
    {
        if (index < 0 || index >= _placeholders.Count)
        {
            Debug.LogError("������������ ������ ������������!");
            return Vector3.zero;
        }
        return _placeholders[index].position;
    }
}