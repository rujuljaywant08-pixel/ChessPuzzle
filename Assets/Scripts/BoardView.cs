using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    public Sprite squareSprite;
    public int width = 4;
    public int height = 5;
    public Color lightColor = new Color(0.96f, 0.9f, 0.7f);
    public Color darkColor = new Color(0.85f, 0.75f, 0.5f);
    public Color dotColor = new Color(0.2f, 0.7f, 0.3f, 0.8f);
    bool levelComplete;

    Piece[,] grid;
    Dictionary<Piece, GameObject> views = new Dictionary<Piece, GameObject>();
    List<GameObject> dots = new List<GameObject>();
    Piece selected;
    List<Vector2Int> currentMoves = new List<Vector2Int>();

    void Start()
    {
        grid = new Piece[width, height];
        DrawBoard();
        MakeTestLevel();
    }

    void Update()
    {
        if (levelComplete) return;

        if (GetClick(out Vector2 screenPos))
        {
            HandleClick(screenPos);
        }
    }

    // Works with mouse in the Editor and touch on the phone
    bool GetClick(out Vector2 screenPos)
    {
#if ENABLE_INPUT_SYSTEM
        var pointer = UnityEngine.InputSystem.Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            screenPos = pointer.position.ReadValue();
            return true;
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
#endif
        screenPos = Vector2.zero;
        return false;
    }

    void HandleClick(Vector2 screenPos)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(screenPos);
        int x = Mathf.RoundToInt(world.x);
        int y = Mathf.RoundToInt(world.y);

        // Clicked outside the board
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            ClearSelection();
            return;
        }

        Vector2Int target = new Vector2Int(x, y);

        // Clicked a green dot: move there
        if (selected != null && currentMoves.Contains(target))
        {
            MovePiece(selected, x, y);
            ClearSelection();
            return;
        }

        // Clicked a white piece: select it
        Piece clicked = grid[x, y];
        if (clicked != null && clicked.isWhite)
            Select(clicked);
        else
            ClearSelection();
    }

    void Select(Piece p)
    {
        ClearSelection();
        selected = p;
        currentMoves = MoveRules.GetLegalMoves(p, grid, width, height);

        foreach (Vector2Int m in currentMoves)
        {
            GameObject dot = new GameObject("Dot");
            dot.transform.parent = transform;
            dot.transform.position = new Vector3(m.x, m.y, 0);
            dot.transform.localScale = new Vector3(0.3f, 0.3f, 1);

            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            sr.color = dotColor;
            sr.sortingOrder = 2;
            dots.Add(dot);
        }
    }

    void ClearSelection()
    {
        selected = null;
        currentMoves.Clear();
        foreach (GameObject d in dots) Destroy(d);
        dots.Clear();
    }

    void MovePiece(Piece p, int x, int y)
    {
        // If there is an enemy piece there, remove it (capture)
        Piece captured = grid[x, y];
        if (captured != null)
        {
            Destroy(views[captured]);
            views.Remove(captured);
        }

        grid[p.x, p.y] = null;
        p.x = x;
        p.y = y;
        grid[x, y] = p;

        views[p].transform.position = new Vector3(x, y, 0);
        if (MoveRules.IsCheckmate(false, grid, width, height))
        {
            levelComplete = true;
            Debug.Log("Checkmate! Level complete!");
        }
    }

    void DrawBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject sq = new GameObject("Square " + x + "," + y);
                sq.transform.parent = transform;
                sq.transform.position = new Vector3(x, y, 0);

                SpriteRenderer sr = sq.AddComponent<SpriteRenderer>();
                sr.sprite = squareSprite;
                sr.color = (x + y) % 2 == 0 ? darkColor : lightColor;
            }
        }

        Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10);
        Camera.main.orthographicSize = height / 2f + 1.5f;
    }

    void MakeTestLevel()
    {
        AddPiece(PieceType.King, false, 3, 4);
        AddPiece(PieceType.Pawn, false, 2, 3);
        AddPiece(PieceType.Pawn, false, 3, 3);
        AddPiece(PieceType.Rook, true,  0, 0);
    }

    void AddPiece(PieceType type, bool isWhite, int x, int y)
    {
        Piece p = new Piece { type = type, isWhite = isWhite, x = x, y = y };
        grid[x, y] = p;
        SpawnPiece(p);
    }

    void SpawnPiece(Piece p)
    {
        string spriteName = (p.isWhite ? "W_" : "B_") + p.type + "_2D";
        Sprite sprite = Resources.Load<Sprite>("Pieces/" + spriteName);

        if (sprite == null)
        {
            Debug.LogError("Sprite not found: " + spriteName);
            return;
        }

        GameObject obj = new GameObject(spriteName);
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(p.x, p.y, 0);

        float size = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float scale = 0.9f / size;
        obj.transform.localScale = new Vector3(scale, scale, 1);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        views[p] = obj;
    }

    void OnGUI()
{
    if (!levelComplete) return;

    GUIStyle style = new GUIStyle(GUI.skin.label);
    style.fontSize = 60;
    style.alignment = TextAnchor.MiddleCenter;
    style.normal.textColor = Color.white;

    GUI.Label(new Rect(0, 100, Screen.width, 120), "Checkmate!", style);
}
}