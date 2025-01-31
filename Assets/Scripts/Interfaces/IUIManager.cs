/// <summary>
/// Интерфейс менеджера UI: обновляет таймер, выводит сообщения, показывает/скрывает меню, работает с рекордами.
/// </summary>
public interface IUIManager
{
    void Tick(float deltaTime);
    void UpdateRemainingMoves(int remainingMoves);
    void ShowWinMessage();
    void ShowLoseMessage();
    void RestartGame();
    void ExitGame();
    void LoadRecords();
    void UpdateRecordsUI();
    void ClearRecords();
    void ShowMenu();
    void HideMenu();
}