using System.Collections.Generic;
using UnityEngine;

public static class MoveRules
{
    static readonly Vector2Int[] RookDirs = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1) };

    static readonly Vector2Int[] BishopDirs = {
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    static readonly Vector2Int[] KnightSteps = {
        new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(2, -1), new Vector2Int(1, -2),
        new Vector2Int(-1, -2), new Vector2Int(-2, -1), new Vector2Int(-2, 1), new Vector2Int(-1, 2) };

    static readonly Vector2Int[] KingSteps = {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    public static List<Vector2Int> GetMoves(Piece p, Piece[,] grid, int w, int h)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (p.type)
        {
            case PieceType.Rook:   Slide(p, grid, w, h, RookDirs, moves); break;
            case PieceType.Bishop: Slide(p, grid, w, h, BishopDirs, moves); break;
            case PieceType.Queen:
                Slide(p, grid, w, h, RookDirs, moves);
                Slide(p, grid, w, h, BishopDirs, moves);
                break;
            case PieceType.Knight: Step(p, grid, w, h, KnightSteps, moves); break;
            case PieceType.King:   Step(p, grid, w, h, KingSteps, moves); break;
            case PieceType.Pawn:   PawnMoves(p, grid, w, h, moves); break;
        }
        return moves;
    }

    static bool Inside(int x, int y, int w, int h)
    {
        return x >= 0 && y >= 0 && x < w && y < h;
    }

    // Rook, bishop, queen: go in a line until something blocks
    static void Slide(Piece p, Piece[,] grid, int w, int h, Vector2Int[] dirs, List<Vector2Int> moves)
    {
        foreach (Vector2Int d in dirs)
        {
            int x = p.x + d.x;
            int y = p.y + d.y;

            while (Inside(x, y, w, h))
            {
                if (grid[x, y] == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (grid[x, y].isWhite != p.isWhite)
                        moves.Add(new Vector2Int(x, y)); // capture
                    break;
                }
                x += d.x;
                y += d.y;
            }
        }
    }

    // Knight and king: one jump to a fixed square
    static void Step(Piece p, Piece[,] grid, int w, int h, Vector2Int[] steps, List<Vector2Int> moves)
    {
        foreach (Vector2Int s in steps)
        {
            int x = p.x + s.x;
            int y = p.y + s.y;

            if (!Inside(x, y, w, h)) continue;
            if (grid[x, y] == null || grid[x, y].isWhite != p.isWhite)
                moves.Add(new Vector2Int(x, y));
        }
    }

    // White pawns go up, black pawns go down
    static void PawnMoves(Piece p, Piece[,] grid, int w, int h, List<Vector2Int> moves)
    {
        int dir = p.isWhite ? 1 : -1;

        int fy = p.y + dir;
        if (Inside(p.x, fy, w, h) && grid[p.x, fy] == null)
            moves.Add(new Vector2Int(p.x, fy));

        for (int dx = -1; dx <= 1; dx += 2)
        {
            int cx = p.x + dx;
            if (Inside(cx, fy, w, h) && grid[cx, fy] != null && grid[cx, fy].isWhite != p.isWhite)
                moves.Add(new Vector2Int(cx, fy));
        }
    }

    // ---------- Check and checkmate ----------

    public static bool IsInCheck(bool whiteKing, Piece[,] grid, int w, int h)
    {
        Vector2Int kingPos = new Vector2Int(-1, -1);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (grid[x, y] != null && grid[x, y].type == PieceType.King && grid[x, y].isWhite == whiteKing)
                    kingPos = new Vector2Int(x, y);

        if (kingPos.x < 0) return false;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Piece enemy = grid[x, y];
                if (enemy == null || enemy.isWhite == whiteKing) continue;

                foreach (Vector2Int m in GetMoves(enemy, grid, w, h))
                    if (m == kingPos) return true;
            }
        }
        return false;
    }

    // Moves that do NOT leave your own king in check
    public static List<Vector2Int> GetLegalMoves(Piece p, Piece[,] grid, int w, int h)
    {
        List<Vector2Int> legal = new List<Vector2Int>();

        foreach (Vector2Int m in GetMoves(p, grid, w, h))
        {
            int oldX = p.x, oldY = p.y;
            Piece captured = grid[m.x, m.y];
            grid[oldX, oldY] = null;
            grid[m.x, m.y] = p;
            p.x = m.x; p.y = m.y;

            bool safe = !IsInCheck(p.isWhite, grid, w, h);

            p.x = oldX; p.y = oldY;
            grid[oldX, oldY] = p;
            grid[m.x, m.y] = captured;

            if (safe) legal.Add(m);
        }
        return legal;
    }

    public static bool HasLegalMove(bool white, Piece[,] grid, int w, int h)
    {
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (grid[x, y] != null && grid[x, y].isWhite == white)
                    if (GetLegalMoves(grid[x, y], grid, w, h).Count > 0)
                        return true;
        return false;
    }

    public static bool IsCheckmate(bool white, Piece[,] grid, int w, int h)
    {
        return IsInCheck(white, grid, w, h) && !HasLegalMove(white, grid, w, h);
    }
}