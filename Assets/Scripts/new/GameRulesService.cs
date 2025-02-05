public class GameRulesService : IGameRulesService
{
    public bool IsValidMove(Ring ring, Tower targetTower)
    {
        // Проверяем, можно ли переместить кольцо на целевую башню
        return targetTower.CanPlaceRing(ring);
    }
}