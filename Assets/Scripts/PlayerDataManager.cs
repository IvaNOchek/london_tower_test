/*using System;
using System.IO;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private const string PlayerDataFileName = "PlayerData.json";
    private PlayerData _playerData;

    public PlayerData CurrentPlayerData => _playerData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);

        if (File.Exists(path))
        {
            // ���� ������ ������ ��� ����������, ��������� ��
            string json = File.ReadAllText(path);
            _playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            // ���� ������ ���, ������� ����� ������� ������
            _playerData = new PlayerData
            {
                PlayerID = Guid.NewGuid().ToString(), // ��������� ����������� ID
                BestMoves = 0,
                BestTime = 0
            };

            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(_playerData, true);
        string path = Path.Combine(Application.persistentDataPath, PlayerDataFileName);
        File.WriteAllText(path, json);
    }

    public void UpdateBestMoves(int moves)
    {
        if (_playerData.BestMoves == 0 || moves < _playerData.BestMoves)
        {
            _playerData.BestMoves = moves;
            SavePlayerData();
        }
    }

    public void UpdateBestTime(float time)
    {
        if (_playerData.BestTime == 0 || time < _playerData.BestTime)
        {
            _playerData.BestTime = time;
            SavePlayerData();
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string PlayerID; // ���������� ID ������
    public int BestMoves;   // ������ ���������� �����
    public float BestTime;  // ������ �����
}*/