using System;
using System.Collections.Generic;

/// <summary>
/// Контейнер для списка всех рекордов (используется для сериализации в JSON)
/// </summary>
[Serializable]
public class RecordsData
{
    public List<GameRecord> AllRecords = new List<GameRecord>();
}