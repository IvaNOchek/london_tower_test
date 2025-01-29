using UnityEngine;
using Zenject;

public class GameTimer : MonoBehaviour
{
    private IUIManager _uiManager;

    [Inject]
    public void Construct(IUIManager uiManager)
    {
        _uiManager = uiManager;
    }

    void Update()
    {
        _uiManager.Tick(Time.deltaTime);
    }
}