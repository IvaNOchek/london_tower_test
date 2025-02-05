public class GameRulesService : IGameRulesService
{
    public bool IsValidMove(Ring ring, Tower targetTower)
    {
        // ���������, ����� �� ����������� ������ �� ������� �����
        return targetTower.CanPlaceRing(ring);
    }
}