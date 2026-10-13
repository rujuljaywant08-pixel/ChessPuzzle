using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;

    Image background;
    Image pieceImage;
    Image dot;
    System.Action<Cell> onClicked;

    public void Setup(int x, int y, float size, Vector2 pos, Color color, Color dotColor, System.Action<Cell> onClicked)
    {
        this.x = x;
        this.y = y;
        this.onClicked = onClicked;

        RectTransform rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = pos;

        background = gameObject.AddComponent<Image>();
        background.color = color;

        pieceImage = MakeChild("Piece", size * 0.9f, Color.white);
        pieceImage.preserveAspect = true;
        pieceImage.enabled = false;

        dot = MakeChild("Dot", size * 0.3f, dotColor);
        dot.enabled = false;
    }

    Image MakeChild(string childName, float size, Color color)
    {
        GameObject go = new GameObject(childName, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        ((RectTransform)go.transform).sizeDelta = new Vector2(size, size);

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false; // so the click goes to the cell
        return img;
    }

    public void SetPiece(Sprite sprite)
    {
        pieceImage.sprite = sprite;
        pieceImage.enabled = sprite != null;
    }

    public void ShowDot(bool show)
    {
        dot.enabled = show;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClicked != null) onClicked(this);
    }
}