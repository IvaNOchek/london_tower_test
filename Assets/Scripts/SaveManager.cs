using UnityEngine;

public class SaveManager : ISaveManager
{
    public void SaveGameResult(int moves, float time, bool isWin)
    {
        if (isWin)
        {
            if (PlayerPrefs.HasKey("MinMoves"))
            {
                int savedMinMoves = PlayerPrefs.GetInt("MinMoves");
                if (moves < savedMinMoves)
                {
                    PlayerPrefs.SetInt("MinMoves", moves);
                }
            }
            else
            {
                PlayerPrefs.SetInt("MinMoves", moves);
            }

            if (PlayerPrefs.HasKey("BestTime"))
            {
                float savedBestTime = PlayerPrefs.GetFloat("BestTime");
                if (time < savedBestTime)
                {
                    PlayerPrefs.SetFloat("BestTime", time);
                }
            }
            else
            {
                PlayerPrefs.SetFloat("BestTime", time);
            }

            PlayerPrefs.Save();
        }
    }

    public GameRecord LoadBestRecord()
    {
        if (PlayerPrefs.HasKey("MinMoves") && PlayerPrefs.HasKey("BestTime"))
        {
            return new GameRecord
            {
                BestMoves = PlayerPrefs.GetInt("MinMoves"),
                BestTime = PlayerPrefs.GetFloat("BestTime")
            };
        }
        return null;
    }

    public void ClearRecords()
    {
        PlayerPrefs.DeleteKey("MinMoves");
        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.Save();
    }
}