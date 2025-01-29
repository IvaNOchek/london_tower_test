using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public int Moves { get; set; }
    public int MaxMoves { get; set; }
    public bool IsGameOver { get; set; }
    public List<Color> TargetColorOrder { get; set; }
}
