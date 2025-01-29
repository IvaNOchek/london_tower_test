using UnityEngine;
using Zenject;

public class GameLoop : MonoBehaviour
{
    private readonly IUIManager _uiManager;
    private readonly IGameManager _gameManager;

    [Inject]
    public GameLoop(IUIManager uiManager, IGameManager gameManager)
    {
        _uiManager = uiManager;
        _gameManager = gameManager;
    }

    void Update()
    {
        _uiManager.Tick(Time.deltaTime);
    }
}