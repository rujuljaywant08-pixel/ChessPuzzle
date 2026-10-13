using UnityEngine;

[System.Serializable]
public class PieceData
{
    public PieceType type;
    public bool isWhite;
    public Vector2Int pos;
}

[CreateAssetMenu(fileName = "Level", menuName = "Chess/Level")]
public class LevelData : ScriptableObject
{
    public int width = 5;
    public int height = 5;
    public PieceData[] pieces;
}