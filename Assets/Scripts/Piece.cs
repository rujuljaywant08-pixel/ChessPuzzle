public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }

[System.Serializable]
public class Piece
{
    public PieceType type;
    public bool isWhite;
    public int x;
    public int y;
}