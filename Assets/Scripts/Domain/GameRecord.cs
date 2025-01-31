using System;

/// <summary>
/// Структура для хранения лучшего результата (по ходам и времени) для конкретного числа башен
/// </summary>
[Serializable]
public class GameRecord
{
    public int TowerCount;  // Количество башен
    public int BestMoves;   // Лучший результат по ходам
    public float BestTime;  // Лучший результат по времени
}