using UnityEngine;
using UnityEngine.UI;

public class HighlightTile : MonoBehaviour
{
    public int x;
    public int y;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        BoardManager.Instance.MoveSelectedPiece(x, y);
    }
}