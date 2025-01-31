using System;

/// <summary>
/// Интерфейс основного игрового менеджера.
/// Управляет ходами, выбором кольца, перемещением, проверкой победы/проигрыша.
/// </summary>
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