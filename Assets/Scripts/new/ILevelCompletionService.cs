using System.Collections.Generic;
public interface ILevelCompletionService
{
    bool IsLevelComplete(List<Tower> towers, GameLevel level);
}