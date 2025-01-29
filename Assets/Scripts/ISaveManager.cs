public interface ISaveManager
{
    void SaveGameResult(int moves, float time, bool isWin);
    GameRecord LoadBestRecord();
    void ClearRecords();
}