public interface IRecordService
{
    void SaveRecord(GameResult result, int numberOfTowers);
    GameResult GetRecord(int numberOfTowers);
    GameResult[] LoadRecords();
}