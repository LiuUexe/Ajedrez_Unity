using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("References")]
    public Transform highlightsParent;
    public GameObject highlightPrefab;

    [Header("Turn")]
    public PieceColor currentTurn = PieceColor.White;

    [Header("Highlight Colors")]
    public Color normalMoveColor = new Color(0f, 0f, 0f, 0.35f);
    public Color captureMoveColor = new Color(1f, 0f, 0f, 0.45f);

    private ChessPiece selectedPiece;
    private readonly List<GameObject> activeHighlights = new List<GameObject>();

    private ChessPiece[,] board = new ChessPiece[8, 8];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RegisterPieces();
        ClearHighlights();
    }

    private void RegisterPieces()
    {
        board = new ChessPiece[8, 8];

        ChessPiece[] pieces = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);

        foreach (ChessPiece piece in pieces)
        {
            if (IsInsideBoard(piece.x, piece.y))
            {
                board[piece.x, piece.y] = piece;
                piece.SetPosition(piece.x, piece.y);
            }
        }
    }

    public void SelectPiece(ChessPiece piece)
    {
        if (piece == null) return;

        if (piece.color != currentTurn)
        {
            Debug.Log("No es el turno de esta pieza");
            return;
        }

        if (selectedPiece != null)
            selectedPiece.SetSelected(false);

        selectedPiece = piece;
        selectedPiece.SetSelected(true);

        ClearHighlights();

        List<Vector2Int> moves = GetLegalMoves(piece);

        foreach (Vector2Int move in moves)
        {
            bool isCapture = board[move.x, move.y] != null;
            CreateHighlight(move.x, move.y, isCapture);
        }
    }

    public void MoveSelectedPiece(int x, int y)
    {
        if (selectedPiece == null) return;

        List<Vector2Int> legalMoves = GetLegalMoves(selectedPiece);

        if (!ContainsMove(legalMoves, x, y))
        {
            Debug.Log("Movimiento ilegal");
            return;
        }

        board[selectedPiece.x, selectedPiece.y] = null;

        ChessPiece targetPiece = board[x, y];

        if (targetPiece != null && targetPiece.color != selectedPiece.color)
        {
            Destroy(targetPiece.gameObject);
        }

        selectedPiece.SetPosition(x, y);
        board[x, y] = selectedPiece;

        selectedPiece.SetSelected(false);
        selectedPiece = null;

        ClearHighlights();
        ChangeTurn();

        CheckGameState();
    }

    private void CheckGameState()
    {
        if (IsKingInCheck(currentTurn))
        {
            if (IsCheckmate(currentTurn))
            {
                Debug.Log("JAQUE MATE. Ganan las " + OppositeColor(currentTurn));
            }
            else
            {
                Debug.Log("JAQUE a " + currentTurn);
            }
        }
        else
        {
            if (IsStalemate(currentTurn))
            {
                Debug.Log("TABLAS por ahogado");
            }
        }
    }

    private List<Vector2Int> GetLegalMoves(ChessPiece piece)
    {
        List<Vector2Int> rawMoves = GetRawMoves(piece);
        List<Vector2Int> legalMoves = new List<Vector2Int>();

        foreach (Vector2Int move in rawMoves)
        {
            if (!WouldLeaveKingInCheck(piece, move.x, move.y))
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;
    }

    private bool WouldLeaveKingInCheck(ChessPiece piece, int targetX, int targetY)
    {
        int originalX = piece.x;
        int originalY = piece.y;

        ChessPiece capturedPiece = board[targetX, targetY];

        board[originalX, originalY] = null;
        board[targetX, targetY] = piece;

        piece.x = targetX;
        piece.y = targetY;

        bool kingInCheck = IsKingInCheck(piece.color);

        piece.x = originalX;
        piece.y = originalY;

        board[originalX, originalY] = piece;
        board[targetX, targetY] = capturedPiece;

        return kingInCheck;
    }

    private bool IsKingInCheck(PieceColor kingColor)
    {
        Vector2Int kingPos = FindKing(kingColor);

        if (kingPos.x == -1)
            return false;

        PieceColor enemyColor = OppositeColor(kingColor);

        ChessPiece[] pieces = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null) continue;
            if (piece.color != enemyColor) continue;

            List<Vector2Int> enemyMoves = GetRawMoves(piece, true);

            foreach (Vector2Int move in enemyMoves)
            {
                if (move.x == kingPos.x && move.y == kingPos.y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Vector2Int FindKing(PieceColor color)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = board[x, y];

                if (piece != null && piece.color == color && piece.type == PieceType.King)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private bool IsCheckmate(PieceColor color)
    {
        if (!IsKingInCheck(color))
            return false;

        return !HasAnyLegalMove(color);
    }

    private bool IsStalemate(PieceColor color)
    {
        if (IsKingInCheck(color))
            return false;

        return !HasAnyLegalMove(color);
    }

    private bool HasAnyLegalMove(PieceColor color)
    {
        ChessPiece[] pieces = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);

        foreach (ChessPiece piece in pieces)
        {
            if (piece == null) continue;
            if (piece.color != color) continue;

            if (GetLegalMoves(piece).Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    private List<Vector2Int> GetRawMoves(ChessPiece piece, bool forAttackCheck = false)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        switch (piece.type)
        {
            case PieceType.Pawn:
                GetPawnMoves(piece, moves, forAttackCheck);
                break;

            case PieceType.Rook:
                GetLineMoves(piece, moves, 1, 0);
                GetLineMoves(piece, moves, -1, 0);
                GetLineMoves(piece, moves, 0, 1);
                GetLineMoves(piece, moves, 0, -1);
                break;

            case PieceType.Bishop:
                GetLineMoves(piece, moves, 1, 1);
                GetLineMoves(piece, moves, 1, -1);
                GetLineMoves(piece, moves, -1, 1);
                GetLineMoves(piece, moves, -1, -1);
                break;

            case PieceType.Queen:
                GetLineMoves(piece, moves, 1, 0);
                GetLineMoves(piece, moves, -1, 0);
                GetLineMoves(piece, moves, 0, 1);
                GetLineMoves(piece, moves, 0, -1);
                GetLineMoves(piece, moves, 1, 1);
                GetLineMoves(piece, moves, 1, -1);
                GetLineMoves(piece, moves, -1, 1);
                GetLineMoves(piece, moves, -1, -1);
                break;

            case PieceType.Knight:
                GetKnightMoves(piece, moves);
                break;

            case PieceType.King:
                GetKingMoves(piece, moves);
                break;
        }

        return moves;
    }

    private void GetPawnMoves(ChessPiece piece, List<Vector2Int> moves, bool forAttackCheck)
    {
        int direction = piece.color == PieceColor.White ? 1 : -1;

        int attackY = piece.y + direction;

        if (forAttackCheck)
        {
            AddPawnAttack(moves, piece.x - 1, attackY);
            AddPawnAttack(moves, piece.x + 1, attackY);
            return;
        }

        int oneStepY = piece.y + direction;

        if (IsInsideBoard(piece.x, oneStepY) && board[piece.x, oneStepY] == null)
        {
            moves.Add(new Vector2Int(piece.x, oneStepY));

            bool isStartingRow =
                (piece.color == PieceColor.White && piece.y == 1) ||
                (piece.color == PieceColor.Black && piece.y == 6);

            int twoStepY = piece.y + direction * 2;

            if (isStartingRow && IsInsideBoard(piece.x, twoStepY) && board[piece.x, twoStepY] == null)
            {
                moves.Add(new Vector2Int(piece.x, twoStepY));
            }
        }

        CheckPawnCapture(piece, moves, piece.x - 1, oneStepY);
        CheckPawnCapture(piece, moves, piece.x + 1, oneStepY);
    }

    private void AddPawnAttack(List<Vector2Int> moves, int x, int y)
    {
        if (IsInsideBoard(x, y))
        {
            moves.Add(new Vector2Int(x, y));
        }
    }

    private void CheckPawnCapture(ChessPiece piece, List<Vector2Int> moves, int x, int y)
    {
        if (!IsInsideBoard(x, y)) return;

        ChessPiece target = board[x, y];

        if (target != null && target.color != piece.color)
        {
            moves.Add(new Vector2Int(x, y));
        }
    }

    private void GetLineMoves(ChessPiece piece, List<Vector2Int> moves, int dirX, int dirY)
    {
        int checkX = piece.x + dirX;
        int checkY = piece.y + dirY;

        while (IsInsideBoard(checkX, checkY))
        {
            ChessPiece target = board[checkX, checkY];

            if (target == null)
            {
                moves.Add(new Vector2Int(checkX, checkY));
            }
            else
            {
                if (target.color != piece.color)
                    moves.Add(new Vector2Int(checkX, checkY));

                break;
            }

            checkX += dirX;
            checkY += dirY;
        }
    }

    private void GetKnightMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        int[,] offsets =
        {
            { 1, 2 },
            { 2, 1 },
            { 2, -1 },
            { 1, -2 },
            { -1, -2 },
            { -2, -1 },
            { -2, 1 },
            { -1, 2 }
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            AddMoveIfValid(piece, moves, piece.x + offsets[i, 0], piece.y + offsets[i, 1]);
        }
    }

    private void GetKingMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                AddMoveIfValid(piece, moves, piece.x + dx, piece.y + dy);
            }
        }
    }

    private void AddMoveIfValid(ChessPiece piece, List<Vector2Int> moves, int x, int y)
    {
        if (!IsInsideBoard(x, y)) return;

        ChessPiece target = board[x, y];

        if (target == null || target.color != piece.color)
        {
            moves.Add(new Vector2Int(x, y));
        }
    }

    private void CreateHighlight(int x, int y, bool isCapture)
    {
        GameObject highlight = Instantiate(highlightPrefab, highlightsParent);

        highlight.transform.SetAsLastSibling();

        RectTransform rect = highlight.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(
            -350 + x * 100,
            -350 + y * 100
        );

        Image image = highlight.GetComponent<Image>();

        if (image != null)
        {
            image.color = isCapture ? captureMoveColor : normalMoveColor;
        }

        HighlightTile tile = highlight.GetComponent<HighlightTile>();

        if (tile != null)
        {
            tile.x = x;
            tile.y = y;
        }

        activeHighlights.Add(highlight);
    }

    private void ClearHighlights()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            Destroy(highlight);
        }

        activeHighlights.Clear();
    }

    private void ChangeTurn()
    {
        currentTurn = currentTurn == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        Debug.Log("Turno actual: " + currentTurn);
    }

    private PieceColor OppositeColor(PieceColor color)
    {
        return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private bool ContainsMove(List<Vector2Int> moves, int x, int y)
    {
        foreach (Vector2Int move in moves)
        {
            if (move.x == x && move.y == y)
                return true;
        }

        return false;
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x <= 7 && y >= 0 && y <= 7;
    }
}