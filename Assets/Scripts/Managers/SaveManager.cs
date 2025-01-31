using System.IO;
using UnityEngine;

/// <summary>
/// Читает/пишет records.json в Application.persistentDataPath
/// </summary>
public class SaveManager : ISaveManager
{
    private readonly string _jsonPath;

    public SaveManager()
    {
        _jsonPath = Path.Combine(Application.persistentDataPath, "records.json");
    }

    public void SaveGameResult(int towerCount, int moves, float time, bool isWin)
    {
        if (!isWin) return;

        RecordsData data = LoadAllRecords() ?? new RecordsData();

        // Ищем подходящую запись
        GameRecord rec = data.AllRecords.Find(r => r.TowerCount == towerCount);
        if (rec == null)
        {
            rec = new GameRecord { TowerCount = towerCount, BestMoves = moves, BestTime = time };
            data.AllRecords.Add(rec);
        }
        else
        {
            // Обновим, если новые результаты лучше
            if (moves < rec.BestMoves) rec.BestMoves = moves;
            if (time < rec.BestTime) rec.BestTime = time;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_jsonPath, json);
    }

    public GameRecord LoadBestRecordFor(int towerCount)
    {
        RecordsData data = LoadAllRecords();
        return data?.AllRecords.Find(r => r.TowerCount == towerCount);
    }

    public RecordsData LoadAllRecords()
    {
        if (!File.Exists(_jsonPath))
            return new RecordsData();

        string json = File.ReadAllText(_jsonPath);
        RecordsData data = JsonUtility.FromJson<RecordsData>(json);
        return data ?? new RecordsData();
    }

    public void ClearRecords()
    {
        if (File.Exists(_jsonPath))
        {
            File.Delete(_jsonPath);
        }
    }
}