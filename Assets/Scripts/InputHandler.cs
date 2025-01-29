using System.Linq;
using UnityEngine;
using Zenject;

public class InputHandler : IInitializable, ITickable
{
    private readonly IGameManager _gameManager;
    private readonly Camera _mainCamera;

    public InputHandler(IGameManager gameManager)
    {
        _gameManager = gameManager;
        _mainCamera = Camera.main;
    }

    public void Initialize()
    {
        // Начальная инициализация, если необходима
    }

    public void Tick()
    {
        HandleRingSelection();
    }

    private void HandleRingSelection()
    {
        if (Input.GetMouseButtonDown(0) && !_gameManager.IsGameOver)
        {

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            // Используем NonAlloc версию для оптимизации
            RaycastHit[] hits = new RaycastHit[5];
            int count = Physics.RaycastNonAlloc(ray, hits);
            if (count > 0)
            {

                if (Physics.Raycast(ray, out RaycastHit hit))
                {

                    Ring clickedRing = hit.collider.GetComponent<Ring>();
                    if (clickedRing != null)
                    {
                        if (clickedRing.IsTransparent)
                        {
                            RingPlaceholder targetPlaceholder = clickedRing.GetComponentInParent<RingPlaceholder>();
                            if (targetPlaceholder != null)
                            {
                                _gameManager.MoveRing(targetPlaceholder);
                            }
                        }
                        else
                        {
                            if (clickedRing.CurrentTower != null && clickedRing.CurrentTower.Rings.LastOrDefault() == clickedRing)
                            {
                                _gameManager.SelectRing(clickedRing);
                            }
                        }
                    }
                    else
                    {
                        _gameManager.SelectRing(null);
                    }
                }
                else
                {
                    _gameManager.SelectRing(null);
                }
            }
        }
    }
}