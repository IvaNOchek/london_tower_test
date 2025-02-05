using UnityEngine;
public class MovementService : IMovementService
{
    public void MoveRing(Ring ring, Tower targetTower)
    {
        if (ring.CurrentTower != null)
        {
            ring.CurrentTower.RemoveRing();
        }
        // Добавляем кольцо на целевую башню
        targetTower.AddRing(ring);
        ring.MoveTo(targetTower, targetTower.GetNextPlaceholderPosition()); // Вызываем метод с анимацией
    }
}