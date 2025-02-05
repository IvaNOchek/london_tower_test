using System.Collections.Generic;
using System.Linq;

public class LevelCompletionService : ILevelCompletionService
{
    public bool IsLevelComplete(List<Tower> towers, GameLevel level)
    {
        // Проверяем, завершен ли уровень
        Tower targetTower = towers[level.TargetTowerIndex];

        // Проверка, что все кольца находятся на целевой башне
        if (targetTower.Rings.Count != level.NumberOfRings)
        {
            return false;
        }

        // Проверка правильного порядка колец (от большего к меньшему)
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