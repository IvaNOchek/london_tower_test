using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Содержит общую информацию об игре, включая кол-во ходов, максимальные ходы, статус завершения и целевой порядок цветов.
/// </summary>
public class GameState
{
    public int Moves { get; set; }
    public int MaxMoves { get; set; }
    public bool IsGameOver { get; set; }
    public List<Color> TargetColorOrder { get; set; }
}