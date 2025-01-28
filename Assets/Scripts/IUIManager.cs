public interface IUIManager
{
    void UpdateRemainingMoves(int remainingMoves);
    void ShowWinMessage();
    void ShowLoseMessage();
    void RestartGame();
    void ExitGame();
    void LoadRecords();
    void UpdateRecordsUI();
    void ClearRecords();
}