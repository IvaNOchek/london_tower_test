/// <summary>
/// Интерфейс для сохранения/загрузки (через JSON) рекордов для разных уровней (количества башен).
/// </summary>
public interface ISaveManager
{
    void SaveGameResult(int towerCount, int moves, float time, bool isWin);
    GameRecord LoadBestRecordFor(int towerCount);
    RecordsData LoadAllRecords();
    void ClearRecords();
}