using UnityEngine;

public class SaveManager : ISaveManager
{
    private const string BestMovesKey = "BestMoves";
    private const string BestTimeKey = "BestTime";

    public void SaveGameResult(int moves, float time, bool isWin)
    {
        if (!isWin) return;

        int currentBestMoves = PlayerPrefs.GetInt(BestMovesKey, int.MaxValue);
        if (moves < currentBestMoves)
            PlayerPrefs.SetInt(BestMovesKey, moves);

        float currentBestTime = PlayerPrefs.GetFloat(BestTimeKey, float.MaxValue);
        if (time < currentBestTime)
            PlayerPrefs.SetFloat(BestTimeKey, time);

        PlayerPrefs.Save();
    }

    public GameRecord LoadBestRecord()
    {
        return new GameRecord
        {
            BestMoves = PlayerPrefs.GetInt(BestMovesKey, 0),
            BestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f)
        };
    }

    public void ClearRecords()
    {
        PlayerPrefs.DeleteKey(BestMovesKey);
        PlayerPrefs.DeleteKey(BestTimeKey);
        PlayerPrefs.Save();
    }
}