using Array2DEditor;
using UnityEngine;

public enum PieceType
{
    PierreFeuilleCiseaux,
    TalkToLong,
    Fireblast,
    Healer,
    Bomber,
    Pointeur,
    CoupDePied,
    Parpaing
}

public enum PieceTeam
{
    Red,
    Blue
}

public class ChessPiece : MonoBehaviour
{
    #region Serialized Fields
    [Header("Chess Piece Settings")]
    public bool isSelectedToMove = false;
    public Vector2 boardPosition;
    public GameObject debugCube;

    [Space(10)]
    [SerializeField] private Array2DBool movementShape = null;
    [SerializeField] private Array2DBool attackShape = null;

    [Space(10)]
    [Tooltip("Automatically defined")]
    public string pieceName;
    [Tooltip("Automatically defined")]
    public unity4dv.Plugin4DS piece4DS;

    [Space(10)]
    [Tooltip("Manually defined")]
    public PieceType pieceType;
    [Tooltip("Manually defined")]
    public PieceTeam pieceTeam;
    #endregion

    #region Initialization
    public void Initialize()
    {
        InitializeDebugCube();
        InitializePieceData();
        ApplyTeamRotation();
    }

    private void InitializeDebugCube()
    {
        if (debugCube != null)
        {
            Color teamColor = (pieceTeam == PieceTeam.Blue) ? Color.blue : Color.red;
            debugCube.GetComponent<Renderer>().material.color = teamColor;
            debugCube.SetActive(false);
        }
    }

    private void InitializePieceData()
    {
        pieceName = gameObject.name;
        piece4DS = GetComponentInChildren<unity4dv.Plugin4DS>();
        
        if (piece4DS != null)
        {
            Debug.Log($"[Init] Piece '{pieceName}' loaded with sequence: {piece4DS.SequenceName}");
        }
    }

    private void ApplyTeamRotation()
    {
        if (pieceTeam == PieceTeam.Red)
        {
            transform.Rotate(0, 0, 180);
            Debug.Log($"[Init] Red team piece '{pieceName}' rotated 180° on Z axis");
        }
    }
    #endregion

    #region Turn Management
    public void PionTurn(bool isTurn)
    {
        isSelectedToMove = isTurn;
        UpdateDebugCube(isTurn);
        UpdateCollider(isTurn);
        
        if (isTurn)
        {
            HighlightTiles();
        }
    }

    private void UpdateDebugCube(bool isActive)
    {
        if (debugCube != null)
        {
            debugCube.SetActive(isActive);
        }
    }

    private void UpdateCollider(bool isEnabled)
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = isEnabled;
        }
    }
    #endregion

    #region Tile Highlighting
    private void HighlightTiles()
    {
        if (!isSelectedToMove)
        {
            return;
        }

        foreach (ChessTile tile in GameManager.Instance.miniChessboard.chessTiles)
        {
            ProcessTileHighlight(tile);
        }
    }

    private void ProcessTileHighlight(ChessTile tile)
    {
        Vector2Int deltaPosition = CalculateDeltaPosition(tile);
        
        bool canMove = CanMoveToTile(deltaPosition);
        bool canAttack = CanAttackTile(deltaPosition);

        if (canMove)
        {
            HighlightMovementTile(tile);
        }

        if (canAttack)
        {
            HighlightAttackTile(tile);
        }
    }

    private Vector2Int CalculateDeltaPosition(ChessTile tile)
    {
        int deltaX = (int)tile.position.x - (int)boardPosition.x;
        int deltaY = (int)tile.position.y - (int)boardPosition.y;
        return new Vector2Int(deltaX, deltaY);
    }

    private bool CanMoveToTile(Vector2Int delta)
    {
        int shapeIndexX = (movementShape.GridSize.x / 2) + delta.x;
        int shapeIndexY = (movementShape.GridSize.y / 2) + delta.y;

        bool isInBounds = IsInBounds(shapeIndexX, shapeIndexY, movementShape.GridSize);
        
        if (!isInBounds)
        {
            return false;
        }

        return movementShape.GetCell(shapeIndexX, shapeIndexY);
    }

    private bool CanAttackTile(Vector2Int delta)
    {
        int shapeIndexX = (attackShape.GridSize.x / 2) + delta.x;
        int shapeIndexY = (attackShape.GridSize.y / 2) + delta.y;

        bool isInBounds = IsInBounds(shapeIndexX, shapeIndexY, attackShape.GridSize);
        
        if (!isInBounds)
        {
            return false;
        }

        return attackShape.GetCell(shapeIndexX, shapeIndexY);
    }

    private bool IsInBounds(int x, int y, Vector2Int gridSize)
    {
        return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
    }

    private void HighlightMovementTile(ChessTile tile)
    {
        ChessPiece pieceAtTile = FindPieceAtTile(tile.position);

        if (pieceAtTile != null && pieceAtTile.pieceTeam == this.pieceTeam)
        {
            // Friendly piece blocks movement
            tile.GetComponent<Renderer>().material.color = tile.baseColor;
            tile.Disable();
        }
        else if (pieceAtTile == null)
        {
            // Empty tile - can move here
            tile.GetComponent<Renderer>().material.color = Color.yellow;
            tile.Enable();
        }
    }

    private void HighlightAttackTile(ChessTile tile)
    {
        ChessPiece pieceAtTile = FindPieceAtTile(tile.position);

        if (pieceAtTile != null && pieceAtTile.pieceTeam != this.pieceTeam)
        {
            // Enemy piece - can attack
            tile.GetComponent<Renderer>().material.color = Color.red;
            tile.Enable();
        }
        else if (pieceAtTile != null && pieceAtTile.pieceTeam == this.pieceTeam)
        {
            // Friendly piece - cannot attack
            tile.GetComponent<Renderer>().material.color = tile.baseColor;
            tile.Disable();
        }
    }

    private ChessPiece FindPieceAtTile(Vector2 tilePosition)
    {
        foreach (ChessPiece piece in GameManager.Instance.miniChessboard.chessPieces)
        {
            if (piece.boardPosition == tilePosition)
            {
                return piece;
            }
        }
        return null;
    }
    #endregion
}