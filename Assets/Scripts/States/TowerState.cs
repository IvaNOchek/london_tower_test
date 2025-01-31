using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранит информацию для одной башни: вместимость и какие кольца лежат
/// </summary>
public class TowerState
{
    public int Capacity { get; }
    public List<Ring> Rings { get; } = new List<Ring>();

    public TowerState(int capacity)
    {
        Capacity = capacity;
    }
}