using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MatchedChessPieces
{
    public ChessPiece smallPiece;
    public ChessPiece giantPiece;
}


public class GameManager : MonoBehaviour
{
    #region Singleton
    static public GameManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Chessboard References")]
    [SerializeField] public ChessboardManager miniChessboard;
    [SerializeField] public ChessboardManager bigChessboard;

    [Header("Player Reference")]
    [SerializeField] public GameObject player;

    [Header("Matched Pieces (Debug Only)")]
    [SerializeField] public List<MatchedChessPieces> matchedPieces = new List<MatchedChessPieces>();
    #endregion

    #region Private Fields
    private ChessPiece lastSelectedPiece = null;
    private bool isBlueTurn = true;
    private Dictionary<ChessPiece, ChessPiece> smallToGiantPieceLookup = new Dictionary<ChessPiece, ChessPiece>();
    #endregion

    #region Constants
    private const float TURN_TRANSITION_TIME = 2.0f;
    private const float GIANT_ANIMATION_DURATION = 2.0f;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    public void MatchSmallAndGiantPieces()
    {
        matchedPieces.Clear();
        smallToGiantPieceLookup.Clear();

        if (!ValidateChessboards())
        {
            return;
        }

        int matchCount = 0;
        foreach (ChessPiece smallPiece in miniChessboard.chessPieces)
        {
            ChessPiece giantPiece = FindMatchingGiantPiece(smallPiece);
            
            if (giantPiece != null)
            {
                MatchedChessPieces match = new MatchedChessPieces
                {
                    smallPiece = smallPiece,
                    giantPiece = giantPiece
                };
                
                matchedPieces.Add(match);
                smallToGiantPieceLookup[smallPiece] = giantPiece;
                matchCount++;
                
                Debug.Log($"[Match {matchCount}] Small: {smallPiece.pieceName} <-> Giant: {giantPiece.pieceName}");
            }
            else
            {
                Debug.LogWarning($"No matching giant piece found for small piece: {smallPiece.pieceName}");
            }
        }

        Debug.Log($"=== Matching Complete: {matchCount} pairs matched ===");
    }

    private ChessPiece FindMatchingGiantPiece(ChessPiece smallPiece)
    {
        foreach (ChessPiece giantPiece in bigChessboard.chessPieces)
        {
            if (ArePiecesMatching(smallPiece, giantPiece))
            {
                return giantPiece;
            }
        }
        return null;
    }

    private bool ArePiecesMatching(ChessPiece piece1, ChessPiece piece2)
    {
        return piece1.pieceName == piece2.pieceName &&
               piece1.pieceType == piece2.pieceType &&
               piece1.pieceTeam == piece2.pieceTeam;
    }

    private bool ValidateChessboards()
    {
        if (bigChessboard == null || miniChessboard == null)
        {
            Debug.LogError("Cannot match pieces: One or both chessboards are null!");
            return false;
        }

        if (miniChessboard.chessPieces.Count == 0)
        {
            Debug.LogWarning("Mini chessboard has no pieces to match.");
            return false;
        }

        if (bigChessboard.chessPieces.Count == 0)
        {
            Debug.LogWarning("Giant chessboard has no pieces to match.");
            return false;
        }

        return true;
    }
    #endregion

    #region Turn Management
    public void MakePionPlayMove()
    {
        if (miniChessboard.chessPieces.Count == 0)
        {
            Debug.LogWarning("No pieces available to make a move.");
            return;
        }

        ChessPiece selectedPiece = SelectRandomPiece();
        
        if (!IsValidPieceSelection(selectedPiece))
        {
            MakePionPlayMove(); // Retry with different piece
            return;
        }

        ResetAllTileHighlights();
        UpdatePieceSelection(selectedPiece);
    }

    private ChessPiece SelectRandomPiece()
    {
        int randomIndex = Random.Range(0, miniChessboard.chessPieces.Count);
        return miniChessboard.chessPieces[randomIndex];
    }

    private bool IsValidPieceSelection(ChessPiece piece)
    {
        if (piece == null || piece.pieceName == "null")
        {
            return false;
        }

        // Prevent selecting the same piece twice in a row (unless it's the only piece)
        if (lastSelectedPiece != null && miniChessboard.chessPieces.Count > 1)
        {
            bool isSamePiece = lastSelectedPiece == piece;
            bool isSameTeam = lastSelectedPiece.pieceTeam == piece.pieceTeam;
            
            if (isSamePiece || isSameTeam)
            {
                return false;
            }
        }

        return true;
    }

    private void ResetAllTileHighlights()
    {
        foreach (ChessTile tile in miniChessboard.chessTiles)
        {
            tile.GetComponent<Renderer>().material.color = tile.baseColor;
            tile.Disable();
        }
    }

    private void UpdatePieceSelection(ChessPiece selectedPiece)
    {
        foreach (ChessPiece piece in miniChessboard.chessPieces)
        {
            bool shouldBeSelected = (piece.pieceName == selectedPiece.pieceName);
            piece.PionTurn(shouldBeSelected);
            
            if (shouldBeSelected)
            {
                lastSelectedPiece = piece;
                Debug.Log($"=== Turn Start: {piece.pieceName} ({piece.pieceTeam}) at position ({piece.boardPosition.x}, {piece.boardPosition.y}) ===");
            }
        }
    }

    public IEnumerator WaitAndNextTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        StopAllPieceAnimations();
        SwitchTurn();
        MakePionPlayMove();
    }

    private void StopAllPieceAnimations()
    {
        StopAnimationsForBoard(miniChessboard);
        
        if (bigChessboard != null)
        {
            StopAnimationsForBoard(bigChessboard);
        }
    }

    private void StopAnimationsForBoard(ChessboardManager board)
    {
        foreach (ChessPiece piece in board.chessPieces)
        {
            if (piece.piece4DS != null)
            {
                piece.piece4DS.Play(false);
                piece.piece4DS.GoToFrame(piece.piece4DS.FirstActiveFrame);
            }
        }
    }

    private void SwitchTurn()
    {
        isBlueTurn = !isBlueTurn;
        Debug.Log($"--- Turn switched to {(isBlueTurn ? "Blue" : "Red")} ---");
    }
    #endregion

    #region Synchronization
    public void SyncGiantChessboard(ChessPiece smallPiece)
    {
        if (!ValidateSyncInput(smallPiece))
        {
            return;
        }

        // Use lookup dictionary for faster matching
        if (smallToGiantPieceLookup.TryGetValue(smallPiece, out ChessPiece giantPiece))
        {
            SyncSinglePiece(smallPiece, giantPiece);
            StartCoroutine(PlayGiantPieceAnimation(giantPiece));
        }
        else
        {
            Debug.LogWarning($"No matching giant piece found in lookup for: {smallPiece.pieceName}");
        }
    }

    public void RemovePieceFromGiantChessboard(ChessPiece smallPiece)
    {
        if (smallPiece == null)
        {
            Debug.LogError("Cannot remove: Small piece is null!");
            return;
        }

        if (bigChessboard == null)
        {
            Debug.LogWarning("Cannot remove: Giant chessboard is not assigned!");
            return;
        }

        // Find the matching giant piece using the lookup dictionary
        if (smallToGiantPieceLookup.TryGetValue(smallPiece, out ChessPiece giantPiece))
        {
            Debug.Log($"[Remove] Removing giant piece '{giantPiece.pieceName}' from giant chessboard");
            
            // Remove from the giant chessboard's piece list
            bigChessboard.chessPieces.Remove(giantPiece);
            
            // Deactivate the giant piece GameObject
            giantPiece.gameObject.SetActive(false);
            
            // Remove from the lookup dictionary
            smallToGiantPieceLookup.Remove(smallPiece);
            
            // Remove from matched pieces list
            matchedPieces.RemoveAll(match => match.smallPiece == smallPiece);
            
            Debug.Log($"[Remove] Giant piece '{giantPiece.pieceName}' successfully removed and deactivated");
        }
        else
        {
            Debug.LogWarning($"No matching giant piece found in lookup for captured piece: {smallPiece.pieceName}");
        }
    }

    private bool ValidateSyncInput(ChessPiece piece)
    {
        if (piece == null)
        {
            Debug.LogError("Cannot sync: Small piece is null!");
            return false;
        }

        if (bigChessboard == null)
        {
            Debug.LogWarning("Cannot sync: Giant chessboard is not assigned!");
            return false;
        }

        return true;
    }

    private void SyncSinglePiece(ChessPiece smallPiece, ChessPiece giantPiece)
    {
        Vector2 targetBoardPosition = smallPiece.boardPosition;
        giantPiece.boardPosition = targetBoardPosition;

        ChessTile targetTile = FindTileAtPosition(bigChessboard, targetBoardPosition);

        if (targetTile != null)
        {
            MoveGiantPieceToTile(giantPiece, targetTile);
        }
        else
        {
            Debug.LogError($"Could not find tile at board position ({targetBoardPosition.x}, {targetBoardPosition.y}) on giant chessboard!");
        }
    }

    private ChessTile FindTileAtPosition(ChessboardManager board, Vector2 position)
    {
        foreach (ChessTile tile in board.chessTiles)
        {
            if (tile.position == position)
            {
                return tile;
            }
        }
        return null;
    }

    private void MoveGiantPieceToTile(ChessPiece giantPiece, ChessTile targetTile)
    {
        Vector3 targetPosition = targetTile.centerPosition.transform.position;
        giantPiece.transform.position = targetPosition;
        
        Debug.Log($"[Sync] Giant piece '{giantPiece.pieceName}' moved to board position ({targetTile.position.x}, {targetTile.position.y})");
    }

    private IEnumerator PlayGiantPieceAnimation(ChessPiece giantPiece)
    {
        if (giantPiece.piece4DS != null)
        {

            if(giantPiece.pieceType == PieceType.Bomber)
            {
                TestAnim testAnim = giantPiece.GetComponentInChildren<TestAnim>();
                if (testAnim != null)
                {
                    testAnim.StartCoroutine(testAnim.CouroutineAnimation());
                }
            } 
            else
            {
                giantPiece.piece4DS.Play(true);

            }
            Debug.Log($"[Animation] Playing giant piece '{giantPiece.pieceName}' animation for {GIANT_ANIMATION_DURATION} seconds");
            
            yield return new WaitForSeconds(GIANT_ANIMATION_DURATION);
            
            giantPiece.piece4DS.Play(false);
            giantPiece.piece4DS.GoToFrame(giantPiece.piece4DS.FirstActiveFrame);

            Debug.Log($"[Animation] Stopped giant piece '{giantPiece.pieceName}' animation");
        }
    }
    #endregion

    #region Debug Input
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MakePionPlayMove();
        }
    }
    #endregion
}
