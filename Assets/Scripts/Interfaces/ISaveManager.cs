/// <summary>
/// ��������� ��� ����������/�������� (����� JSON) �������� ��� ������ ������� (���������� �����).
/// </summary>
public interface ISaveManager
{
    void SaveGameResult(int towerCount, int moves, float time, bool isWin);
    GameRecord LoadBestRecordFor(int towerCount);
    RecordsData LoadAllRecords();
    void ClearRecords();
}