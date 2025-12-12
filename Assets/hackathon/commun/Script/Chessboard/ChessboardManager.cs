using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessboardManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Chessboard Components")]
    [SerializeField] public List<ChessTile> chessTiles = new List<ChessTile>();
    [SerializeField] public List<ChessPiece> chessPieces = new List<ChessPiece>();
    [SerializeField] public Transform chessPiecesParent;
    #endregion

    #region Constants
    private const int BOARD_SIZE = 5;
    private const string TILE_PREFIX = "SM_Tile";
    private const float HIGHLIGHT_CLEANUP_DELAY = 0.2f;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeChessboard();
        GameManager.Instance.MatchSmallAndGiantPieces();
    }
    #endregion

    #region Initialization
    private void InitializeChessboard()
    {
        InitializeTiles();
        InitializePieces();
    }

    private void InitializeTiles()
    {
        foreach (Transform child in transform)
        {
            ChessTile tile = child.GetComponent<ChessTile>();
            if (tile != null)
            {
                Vector2Int tilePosition = CalculateTilePosition(child.name);
                tile.Initialize(tilePosition.x, tilePosition.y);
                chessTiles.Add(tile);
            }
        }
        
        Debug.Log($"[{gameObject.name}] Initialized {chessTiles.Count} tiles");
    }

    private Vector2Int CalculateTilePosition(string tileName)
    {
        int index = int.Parse(tileName.Replace(TILE_PREFIX, ""));
        int x = index % BOARD_SIZE;
        int y = index / BOARD_SIZE;
        return new Vector2Int(x, y);
    }

    private void InitializePieces()
    {
        foreach (Transform child in chessPiecesParent)
        {
            ChessPiece piece = child.GetComponent<ChessPiece>();
            if (piece != null)
            {
                piece.Initialize();
                chessPieces.Add(piece);
                
                SetInitialPieceBoardPosition(piece);
            }
        }
        
        Debug.Log($"[{gameObject.name}] Initialized {chessPieces.Count} pieces");
    }

    private void SetInitialPieceBoardPosition(ChessPiece piece)
    {
        ChessTile closestTile = FindClosestTile(piece.transform.position);
        
        if (closestTile != null)
        {
            piece.boardPosition = closestTile.position;
            Debug.Log($"[Init] Piece '{piece.pieceName}' placed at board position ({closestTile.position.x}, {closestTile.position.y})");
        }
        else
        {
            Debug.LogWarning($"Could not find initial tile for piece: {piece.pieceName}");
        }
    }
    #endregion

    #region Piece Movement
    public void UpdateChessPiecePosition(ChessPiece piece)
    {
        if (piece.pieceName == "null")
        {
            Debug.LogWarning("Attempted to move a null piece");
            return;
        }

        ChessTile targetTile = FindClosestSelectableTile(piece.transform.position);

        if (targetTile != null)
        {
            MovePieceToTile(piece, targetTile);
            HandlePieceCapture(piece, targetTile);
            HandlePostMoveActions(piece);
        }
        else
        {
            Debug.LogWarning($"No valid tile found for piece: {piece.pieceName}");
        }
    }


    private void MovePieceToTile(ChessPiece piece, ChessTile tile)
    {
        piece.boardPosition = tile.position;
        Debug.Log($"[Move] '{piece.pieceName}' moved to ({tile.position.x}, {tile.position.y})");
    }

    private void HandlePieceCapture(ChessPiece movingPiece, ChessTile targetTile)
    {
        if (!IsCaptureMove(targetTile))
        {
            return;
        }

        ChessPiece capturedPiece = FindEnemyPieceAt(movingPiece, targetTile.position);
        
        if (capturedPiece != null)
        {
            CapturePiece(movingPiece, capturedPiece);
        }
    }

    private bool IsCaptureMove(ChessTile tile)
    {
        return tile.GetComponent<Renderer>().material.color == Color.red;
    }

    private ChessPiece FindEnemyPieceAt(ChessPiece attackingPiece, Vector2 position)
    {
        foreach (ChessPiece piece in chessPieces)
        {
            bool isEnemy = piece.pieceTeam != attackingPiece.pieceTeam;
            bool isAtPosition = piece.boardPosition == position;
            
            if (isEnemy && isAtPosition)
            {
                return piece;
            }
        }
        return null;
    }

    private void CapturePiece(ChessPiece attacker, ChessPiece victim)
    {
        Debug.Log($"[Capture] '{attacker.pieceName}' captured '{victim.pieceName}'!");
        
        // Sync the capture to the giant chessboard before removing from mini board
        GameManager.Instance.RemovePieceFromGiantChessboard(victim);
        
        victim.gameObject.SetActive(false);
        chessPieces.Remove(victim);
    }

    private void HandlePostMoveActions(ChessPiece piece)
    {
        PlayPieceAnimation(piece);
        //SyncWithGiantBoard(piece);
        GameManager.Instance.SyncGiantChessboard(piece);
        CleanupAndStartNextTurn();
    }

    private void PlayPieceAnimation(ChessPiece piece)
    {
        if (piece.piece4DS != null)
        {
            piece.piece4DS.Play(true);
        }
    }

    private void CleanupAndStartNextTurn()
    {
        StartCoroutine(CleanChessboardHighlight(HIGHLIGHT_CLEANUP_DELAY));
        StartCoroutine(GameManager.Instance.WaitAndNextTurn(2.0f));
    }
    #endregion

    #region Tile Helpers
    private ChessTile FindClosestTile(Vector3 worldPosition)
    {
        float minDistance = float.MaxValue;
        ChessTile closestTile = null;

        foreach (ChessTile tile in chessTiles)
        {
            float distance = Vector3.Distance(worldPosition, tile.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    private ChessTile FindClosestSelectableTile(Vector3 worldPosition)
    {
        float minDistance = float.MaxValue;
        ChessTile closestTile = null;

        foreach (ChessTile tile in chessTiles)
        {
            if (!tile.isSelectable)
            {
                continue;
            }

            float distance = Vector3.Distance(worldPosition, tile.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    public IEnumerator CleanChessboardHighlight(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        foreach (ChessTile tile in chessTiles)
        {
            tile.GetComponent<Renderer>().material.color = tile.baseColor;
            tile.Disable();
        }
    }
    #endregion

    #region Legacy (Keep for compatibility)
    private ChessPiece currentChessPiece;

    public void SetCurrentChessPiece(ChessPiece piece)
    {
        currentChessPiece = piece;
    }
    #endregion
}
