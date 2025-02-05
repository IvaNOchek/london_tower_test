using System.Collections.Generic;
using System.Linq;

public class LevelCompletionService : ILevelCompletionService
{
    public bool IsLevelComplete(List<Tower> towers, GameLevel level)
    {
        // ���������, �������� �� �������
        Tower targetTower = towers[level.TargetTowerIndex];

        // ��������, ��� ��� ������ ��������� �� ������� �����
        if (targetTower.Rings.Count != level.NumberOfRings)
        {
            return false;
        }

        // �������� ����������� ������� ����� (�� �������� � ��������)
        for (int i = 0; i < targetTower.Rings.Count - 1; i++)
        {
            if (targetTower.Rings[i].Size < targetTower.Rings[i + 1].Size)
            {
                return false;
            }
        }

        return true;
    }
}