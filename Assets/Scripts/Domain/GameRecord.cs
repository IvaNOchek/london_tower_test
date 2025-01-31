using System;

/// <summary>
/// ��������� ��� �������� ������� ���������� (�� ����� � �������) ��� ����������� ����� �����
/// </summary>
[Serializable]
public class GameRecord
{
    public int TowerCount;  // ���������� �����
    public int BestMoves;   // ������ ��������� �� �����
    public float BestTime;  // ������ ��������� �� �������
}