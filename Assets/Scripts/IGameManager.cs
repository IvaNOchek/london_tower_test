public interface IGameManager
{
    int Moves { get; }
    void StartNewGame();
    void SelectRing(Ring ring);
    void MoveRing(RingPlaceholder targetPlaceholder);



    // Другие публичные методы и события
}