using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public delegate void RingClickedHandler(Ring ring);
    public event RingClickedHandler OnRingClicked;


    private Camera _mainCamera; // Кэшируем ссылку

    private void Start()
    {
        _mainCamera = Camera.main; // Получаем ссылку при старте
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Левая кнопка мыши / касание
        {
            if (EventSystem.current.IsPointerOverGameObject()) return; // UI elements

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Ring ring = hit.collider.GetComponent<Ring>();
                if (ring != null)
                {
                    OnRingClicked?.Invoke(ring); // Вызываем событие и для обычного, и для призрачного кольца
                    return;
                }
            }
        }
    }
}