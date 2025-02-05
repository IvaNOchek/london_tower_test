public interface IGameResultRepository
{
    void Save(GameResult[] result);
    GameResult[] Load();
}