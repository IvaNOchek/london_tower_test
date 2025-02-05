using UnityEngine;
using Zenject;

public class SolutionDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _imageColorPrefab; // Префаб Image_color

    public void CreateSolutionDisplay(int numberOfRings, Color[] ringColors)
    {
        //Удаляем старые
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Создаем нужное количество Image_color
        for (int i = 0; i < numberOfRings; i++)
        {
            GameObject imageObj = Instantiate(_imageColorPrefab, transform);
            UnityEngine.UI.Image image = imageObj.GetComponent<UnityEngine.UI.Image>();
            image.color = ringColors[i % ringColors.Length]; // Назначаем цвет
        }
    }
}