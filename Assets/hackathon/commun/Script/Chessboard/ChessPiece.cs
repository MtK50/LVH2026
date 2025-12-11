using Array2DEditor;
using Oculus.Platform.Models;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

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
    [Header("Chess Piece Settings")]
    public bool isSelectedToMove = false;
    public Vector2 boardPosition;
    public GameObject debugCube;
    [Space(10)]
    // Create a 5x5 matrix of bool to represent possible moves
    [SerializeField] private Array2DBool movementShape = null;
    [SerializeField] private Array2DBool attackShape = null;


    [Space(10)]


    [Tooltip("Automaticaly defined")]
    public string pieceName;
    [Tooltip("Automaticaly defined")]
    public unity4dv.Plugin4DS piece4DS;
    [Space(10)]
    [Tooltip("Manually defined")]
    public PieceType pieceType;
    [Tooltip("Manually defined")]
    public PieceTeam pieceTeam;


    public void Initialize()
    {
        debugCube.GetComponent<Renderer>().material.color = (pieceTeam == PieceTeam.Blue) ? Color.blue : Color.red;
        debugCube.SetActive(false);

        pieceName = gameObject.name;

        piece4DS = GetComponentInChildren<unity4dv.Plugin4DS>();
        piece4DS.AutoPlay = false;

        Vector3 rot = piece4DS.transform.eulerAngles;
        if (pieceTeam == PieceTeam.Red)
        {
            rot.z += 180f;
        }
        piece4DS.transform.eulerAngles = rot;

    }

    public void PionTurn(bool isTurn)
    {
        isSelectedToMove = isTurn;
        debugCube.SetActive(isTurn);

        GetComponent<Collider>().enabled = isTurn;
        HighlightTiles();

    }

    private void HighlightTiles()
    {
        if (!isSelectedToMove) return;
        foreach (ChessTile tile in GameManager.Instance.miniChessboard.chessTiles)
        {
            // Calculer le déplacement relatif depuis la position de la pièce
            int deltaX = (int)tile.position.x - (int)boardPosition.x;
            int deltaY = (int)tile.position.y - (int)boardPosition.y;

            // Convertir en indices de la matrice shape (le centre = position de la pièce)
            int movementShapeCenterX = movementShape.GridSize.x / 2;
            int movementShapeCenterY = movementShape.GridSize.y / 2;

            int attackShapeCenterX = attackShape.GridSize.x / 2;
            int attackShapeCenterY = attackShape.GridSize.y / 2;

            int movementShapeIndexX = movementShapeCenterX + deltaX;
            int movementShapeIndexY = movementShapeCenterY + deltaY;

            int attackShapeIndexX = attackShapeCenterX + deltaX;
            int attackShapeIndexY = attackShapeCenterY + deltaY;

            bool isInMovementShapeBounds =
                movementShapeIndexX >= 0 && movementShapeIndexX < movementShape.GridSize.x &&
                movementShapeIndexY >= 0 && movementShapeIndexY < movementShape.GridSize.y;

            bool isInAttackShapeBounds =
                attackShapeIndexX >= 0 && attackShapeIndexX < attackShape.GridSize.x &&
                attackShapeIndexY >= 0 && attackShapeIndexY < attackShape.GridSize.y;

            if (isInMovementShapeBounds && movementShape.GetCell(movementShapeIndexX, movementShapeIndexY))
            {
                tile.GetComponent<Renderer>().material.color = Color.yellow;
                tile.Enable();

                foreach (ChessPiece piece in GameManager.Instance.miniChessboard.chessPieces)
                {
                    // BaseColor - Friendly piece on possible move tile
                    if (piece.boardPosition == tile.position)
                    {
                        tile.GetComponent<Renderer>().material.color = tile.baseColor;
                        tile.Disable();
                    }
                }
            }

            if (isInAttackShapeBounds && attackShape.GetCell(attackShapeIndexX, attackShapeIndexY))
            {
                foreach(ChessPiece piece in GameManager.Instance.miniChessboard.chessPieces)
                {
                    // RED - Ennemy piece on possible attack tile
                    if (piece.boardPosition == tile.position && piece.pieceTeam != this.pieceTeam)
                    {
                        tile.GetComponent<Renderer>().material.color = Color.red;
                        tile.Enable();
                    }

                    // BaseColor - Friendly piece on possible move tile
                    if (piece.boardPosition == tile.position && piece.pieceTeam == this.pieceTeam)
                    {
                        tile.GetComponent<Renderer>().material.color = tile.baseColor;
                        tile.Disable();
                    }
                }
            }
        }
    }
}