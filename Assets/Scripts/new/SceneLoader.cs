using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISceneLoader
{
    public void LoadMenuScene()
    {
        SceneManager.LoadSceneAsync("MenuScene");
    }

    public void LoadMainScene(int numberOfTowers)
    {
        PlayerPrefs.SetInt("NumberOfTowers", numberOfTowers);
        PlayerPrefs.Save();
        SceneManager.LoadSceneAsync("MainScene");
        Debug.Log("башня из " + numberOfTowers);
    }
}