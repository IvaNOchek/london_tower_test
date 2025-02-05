using System;
using System.IO;
using UnityEngine;
public class GameResultRepository : IGameResultRepository
{
    private readonly string _filePath;
    private const string FileName = "GameResults.json";
    public GameResultRepository()
    {
        _filePath = Path.Combine(Application.persistentDataPath, FileName);
    }
    public void Save(GameResult[] result)
    {
        string json = JsonHelper.ToJson(result);
        File.WriteAllText(_filePath, json);
    }

    public GameResult[] Load()
    {
        try
        {
            return File.Exists(_filePath)
                ? JsonHelper.FromJson<GameResult>(File.ReadAllText(_filePath))
                : Array.Empty<GameResult>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading records: {e.Message}");
            return Array.Empty<GameResult>();
        }
    }
}