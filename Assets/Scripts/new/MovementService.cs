using UnityEngine;
public class MovementService : IMovementService
{
    public void MoveRing(Ring ring, Tower targetTower)
    {
        if (ring.CurrentTower != null)
        {
            ring.CurrentTower.RemoveRing();
        }
        // ��������� ������ �� ������� �����
        targetTower.AddRing(ring);
        ring.MoveTo(targetTower, targetTower.GetNextPlaceholderPosition()); // �������� ����� � ���������
    }
}