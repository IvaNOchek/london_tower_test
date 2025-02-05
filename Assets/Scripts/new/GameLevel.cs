public class GameLevel
{
    public int NumberOfRings { get; private set; }
    public int MaxMoves { get; private set; }
    public int TargetTowerIndex { get; private set; }

    public GameLevel(int numberOfRings, int maxMoves, int targetTowerIndex)
    {
        NumberOfRings = numberOfRings;
        MaxMoves = maxMoves;
        TargetTowerIndex = targetTowerIndex;
    }
}