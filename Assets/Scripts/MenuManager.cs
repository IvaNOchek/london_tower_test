using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class MenuManager : MonoBehaviour
{
    public int NumTowers = 3;
    public TextMeshProUGUI RecordsText; // Поле для вывода рекордов

    void Start()
    {
        if (RecordsText != null)
        {
            DisplayRecords();
        }
    }

    public void StartGameWithTowers()
    {
        PlayerPrefs.SetInt("SelectedTowers", NumTowers);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }


    public void DisplayRecords()
    {
        string recordsString = "";
        for (int i = 3; i <= 8; i++)
        {
            string fileName = $"GameRecords_{i}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameRecord record = JsonUtility.FromJson<GameRecord>(json);
                if (record != null && record.NumOfTowers == i)
                {
                    string formattedTime = FormatTime(record.BestTime);
                    recordsString += $"Башни: {i}, Ходы: {record.BestMoves}, Время: {formattedTime}\n";
                }
                else
                {
                    recordsString += $"Башни: {i}, Рекорд не найден\n";
                }
            }
            else
            {
                recordsString += $"Башни: {i}, Ходы: -, Время: -\n";
            }
        }
        RecordsText.text = recordsString;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}