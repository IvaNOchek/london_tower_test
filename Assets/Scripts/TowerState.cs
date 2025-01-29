using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerState
{
    public int Capacity { get; }
    public List<Ring> Rings { get; } = new List<Ring>();

    public TowerState(int capacity)
    {
        Capacity = capacity;
    }
}