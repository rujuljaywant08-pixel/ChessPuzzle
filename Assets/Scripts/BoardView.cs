using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    public LevelData[] levels;
    public float cellSize = 180f;
    public Color lightColor = new Color(0.96f, 0.9f, 0.7f);
    public Color darkColor = new Color(0.85f, 0.75f, 0.5f);
    public Color dotColor = new Color(0.2f, 0.7f, 0.3f, 0.8f);

    int width, height, currentLevel;
    bool levelComplete;

    Piece[,] grid;
    Cell[,] cells;
    Piece selected;
    List<Vector2Int> currentMoves = new List<Vector2Int>();

    Text titleText;
    GameObject winPanel;

    void Start()
    {
        BuildUI();
        LoadLevel(0);
    }

    // ---------- Levels ----------

    void LoadLevel(int index)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("No levels assigned on the Board object!");
            return;
        }

        currentLevel = index;
        levelComplete = false;
        winPanel.SetActive(false);
        selected = null;
        currentMoves = new List<Vector2Int>();

        // Remove the old cells
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        LevelData data = levels[index];
        width = data.width;
        height = data.height;
        grid = new Piece[width, height];
        cells = new Cell[width, height];

        titleText.text = "Level " + (index + 1) + "\nMate in 1";

        GenerateGrid();

        foreach (PieceData pd in data.pieces)
        {
            grid[pd.pos.x, pd.pos.y] = new Piece
            {
                type = pd.type,
                isWhite = pd.isWhite,
                x = pd.pos.x,
                y = pd.pos.y
            };
        }

        RefreshPieces();
    }

    void NextLevel()
    {
        int next = currentLevel + 1;
        if (next >= levels.Length) next = 0;
        LoadLevel(next);
    }

    // ---------- Grid generation ----------

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject go = new GameObject("Cell " + x + "," + y, typeof(RectTransform));
                go.transform.SetParent(transform, false);

                Vector2 pos = new Vector2(
                    (x - (width - 1) / 2f) * cellSize,
                    (y - (height - 1) / 2f) * cellSize);

                Color c = (x + y) % 2 == 0 ? darkColor : lightColor;

                Cell cell = go.AddComponent<Cell>();
                cell.Setup(x, y, cellSize, pos, c, dotColor, OnCellClicked);
                cells[x, y] = cell;
            }
        }
    }

    // Show the right sprite on every cell
    void RefreshPieces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece p = grid[x, y];
                cells[x, y].SetPiece(p == null ? null : GetSprite(p));
            }
        }
    }

    Sprite GetSprite(Piece p)
    {
        string spriteName = (p.isWhite ? "W_" : "B_") + p.type + "_2D";
        Sprite s = Resources.Load<Sprite>("Pieces/" + spriteName);
        if (s == null) Debug.LogError("Sprite not found: " + spriteName);
        return s;
    }

    // ---------- Clicking ----------

    void OnCellClicked(Cell cell)
    {
        if (levelComplete) return;

        Vector2Int target = new Vector2Int(cell.x, cell.y);

        // Tapped a green dot: move there
        if (selected != null && currentMoves.Contains(target))
        {
            Piece moving = selected;
            ClearSelection();
            MovePiece(moving, cell.x, cell.y);
            return;
        }

        // Tapped a white piece: select it
        Piece clicked = grid[cell.x, cell.y];
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
            cells[m.x, m.y].ShowDot(true);
    }

    void ClearSelection()
    {
        foreach (Vector2Int m in currentMoves)
            cells[m.x, m.y].ShowDot(false);

        selected = null;
        currentMoves = new List<Vector2Int>();
    }

    void MovePiece(Piece p, int x, int y)
    {
        grid[p.x, p.y] = null;
        p.x = x;
        p.y = y;
        grid[x, y] = p; // this also removes a captured piece

        RefreshPieces();

        if (MoveRules.IsCheckmate(false, grid, width, height))
        {
            levelComplete = true;
            winPanel.SetActive(true);
        }
    }

    // ---------- Simple UI (made by code) ----------

    void BuildUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Board must be a child of a Canvas!");
            return;
        }

        titleText = MakeText(canvas.transform, "Title", "", 60, new Vector2(0, 650));

        winPanel = new GameObject("WinPanel", typeof(RectTransform));
        winPanel.transform.SetParent(canvas.transform, false);

        MakeText(winPanel.transform, "WinText", "Checkmate!", 90, new Vector2(0, -580));
        MakeButton(winPanel.transform, "Next Level", new Vector2(0, -740), NextLevel);

        winPanel.SetActive(false);
    }

    Text MakeText(Transform parent, string objName, string content, int fontSize, Vector2 pos)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;

        t.rectTransform.sizeDelta = new Vector2(900, 200);
        t.rectTransform.anchoredPosition = pos;
        return t;
    }

    void MakeButton(Transform parent, string label, Vector2 pos, UnityAction onClick)
    {
        GameObject go = new GameObject("Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.7f, 0.3f);
        img.rectTransform.sizeDelta = new Vector2(450, 130);
        img.rectTransform.anchoredPosition = pos;

        Button b = go.AddComponent<Button>();
        b.onClick.AddListener(onClick);

        MakeText(go.transform, "Label", label, 55, Vector2.zero);
    }
}