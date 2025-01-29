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