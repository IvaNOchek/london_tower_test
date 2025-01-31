using UnityEngine;
using Zenject;

/// <summary>
/// �������� Tick � UIManager ������ ����.
/// </summary>
public class GameTimer : MonoBehaviour
{
    private IUIManager _uiManager;

    [Inject]
    public void Construct(IUIManager uiManager)
    {
        _uiManager = uiManager;
    }

    private void Update()
    {
        _uiManager.Tick(Time.deltaTime);
    }
}