using UnityEngine;
using UnityEngine.UI;

public enum PieceColor
{
    White,
    Black
}

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public class ChessPiece : MonoBehaviour
{
    public PieceColor color;
    public PieceType type;

    public int x;
    public int y;

    private Button button;
    private Outline outline;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnPieceClicked);

        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(4f, 4f);
        outline.enabled = false;
    }

    private void OnPieceClicked()
    {
        BoardManager.Instance.SelectPiece(this);
    }

    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;

        RectTransform rect = GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(
            -350 + x * 100,
            -350 + y * 100
        );
    }

    public void SetSelected(bool selected)
    {
        if (outline != null)
            outline.enabled = selected;
    }
}