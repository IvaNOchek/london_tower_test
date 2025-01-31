/// <summary>
/// »нтерфейс дл€ управлени€ главным меню (старт игры, выход, показ рекордов, скрыть/показать меню).
/// </summary>
public interface IMenuManager
{
    void StartGameWithTowers(int numTowers);
    void ExitGame();
    void DisplayRecords();
    void ShowMenu();
    void HideMenu();
}