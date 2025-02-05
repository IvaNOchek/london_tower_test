using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RecordService : IRecordService
{
    private readonly string _filePath;
    private const string FileName = "GameResults.json";
    private readonly IGameResultRepository _repository;

    public RecordService(IGameResultRepository repository)
    {
        _repository = repository;
        _filePath = Path.Combine(Application.persistentDataPath, FileName);
        Debug.Log($"JSON path: {_filePath}");
    }

    public void SaveRecord(GameResult result, int numberOfTowers)
    {
        var allRecords = LoadRecords().ToList();
        var existingRecord = allRecords.FirstOrDefault(r => r.Towers == numberOfTowers);

        if (existingRecord == null)
        {
            allRecords.Add(new GameResult(numberOfTowers, result.Moves, result.Time));
        }
        else
        {
            if (result.Moves < existingRecord.Moves ||
               (result.Moves == existingRecord.Moves && result.Time < existingRecord.Time))
            {
                existingRecord.Moves = result.Moves;
                existingRecord.Time = result.Time;
            }
        }

        _repository.Save(allRecords.OrderBy(x => x.Towers).ToArray());
    }

    public GameResult GetRecord(int numberOfTowers)
    {
        GameResult[] allRecords = LoadRecords();
        return allRecords.FirstOrDefault(r => r.Towers == numberOfTowers);
    }

    public GameResult[] LoadRecords()
    {
        return _repository.Load();
    }
}