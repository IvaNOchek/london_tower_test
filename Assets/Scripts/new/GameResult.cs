using System;

[Serializable]
public class GameResult
{
    public int Towers;  //Количество башен
    public int Moves;
    public float Time;

    public GameResult(int towers, int moves, float time)
    {
        Towers = towers;
        Moves = moves;
        Time = time;
    }
}