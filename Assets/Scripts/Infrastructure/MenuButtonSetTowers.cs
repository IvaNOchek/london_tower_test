using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Вешается на кнопку в главное меню. В Inspector указываем towersCount и имя GameScene.
/// При клике сохраняет towersCount и переключается на сцену GameScene.
/// </summary>
public class MenuButtonSetTowers : MonoBehaviour
{
    [SerializeField] private int towersCount = 3;
    [SerializeField] private string sceneToLoad = "MainScene";

    public void OnClick_SetTowersAndLoadGame()
    {
        PlayerPrefs.SetInt("SelectedTowers", towersCount);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneToLoad);
    }
}