using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �������� �� ������ � ������� ����. � Inspector ��������� towersCount � ��� GameScene.
/// ��� ����� ��������� towersCount � ������������� �� ����� GameScene.
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