using System;

public interface IGameManager
{
    int Moves { get; }
    bool IsGameOver { get; }

    void StartNewGame();
    void SelectRing(Ring ring);
    void MoveRing(RingPlaceholder targetPlaceholder);

    event Action<int> OnMovesUpdated;
    event Action OnGameWon;
    event Action OnGameLost;
}