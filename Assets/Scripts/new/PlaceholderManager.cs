using System.Collections.Generic;
using UnityEngine;

public class PlaceholderManager : MonoBehaviour
{
    private List<Transform> _placeholders = new List<Transform>();

    [SerializeField] private GameObject _placeholderPrefab;    // Префаб плейсхолдера

    public void SetupPlaceholders(int capacity, Transform parent)
    {
        _placeholders.Clear();

        float startY = 0.3f;     // Базовое смещение первого плейсхолдера   
        float stepY = 0.55f;     // Шаг между плейсхолдерами в локальном масштабе
        float towerScaleY = parent.localScale.y;  // Получаем масштаб башни по Y

        for (int i = 0; i < capacity; i++)
        {
            GameObject placeholder = Instantiate(_placeholderPrefab, parent);
            placeholder.name = $"Placeholder_{i}";

            // Рассчитываем корректное смещение по Y с учетом масштаба башни
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
            Debug.LogError("Недопустимый индекс плейсхолдера!");
            return Vector3.zero;
        }
        return _placeholders[index].position;
    }
}