using System;
using System.Collections.Generic;

/// <summary>
/// ��������� ��� ������ ���� �������� (������������ ��� ������������ � JSON)
/// </summary>
[Serializable]
public class RecordsData
{
    public List<GameRecord> AllRecords = new List<GameRecord>();
}