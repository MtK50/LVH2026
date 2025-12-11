using Array2DEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] private Array2DBool shape = null;


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
        debugCube.SetActive(false);

        pieceName = gameObject.name;

        piece4DS = GetComponentInChildren<unity4dv.Plugin4DS>();
        Debug.Log(piece4DS.SequenceName);
    }

    public void PionTurn(bool isTurn)
    {
        isSelectedToMove = isTurn;
        debugCube.SetActive(isTurn);

        GetComponent<Collider>().enabled = isTurn;

        HighlightPossibleMove();

    }
    private void HighlightPossibleMove()
    {
        // Ne surligner que si cette pièce est sélectionnée
        if (!isSelectedToMove) return;

        GameManager.Instance.miniChessboard.possibleChessTiles.Clear();

        foreach (ChessTile tile in GameManager.Instance.miniChessboard.chessTiles)
        {
            // Calculer le déplacement relatif depuis la position de la pièce
            int deltaX = (int)tile.position.x - (int)boardPosition.x;
            int deltaY = (int)tile.position.y - (int)boardPosition.y;

            // Convertir en indices de la matrice shape (le centre = position de la pièce)
            int shapeX = deltaX + shape.GridSize.x / 2;
            int shapeY = deltaY + shape.GridSize.y / 2;

            // Vérifier si ce déplacement est dans les limites de la matrice shape
            bool isInShapeBounds = shapeX >= 0 && shapeX < shape.GridSize.x && 
                                    shapeY >= 0 && shapeY < shape.GridSize.y;

            // Surligner uniquement les mouvements valides
            if (isInShapeBounds && shape.GetCell(shapeX, shapeY))
            {

                tile.GetComponent<Renderer>().material.color = Color.yellow;
                //tile.centerPosition.SetActive(true);
                tile.Enable();


                if (tile.position == boardPosition)
                {
                    tile.GetComponent<Renderer>().material.color = Color.green;
                }
                foreach(ChessPiece piece in GameManager.Instance.miniChessboard.chessPieces)
                {
                    if(piece.boardPosition == tile.position)
                    {
                        // RED - Ennemy piece on possible move tile
                        if (piece.pieceTeam != this.pieceTeam)
                        {
                            tile.GetComponent<Renderer>().material.color = Color.red;
                        }

                        // BaseColor - Friendly piece on possible move tile
                        if (piece.pieceTeam == this.pieceTeam)
                        {
                            tile.GetComponent<Renderer>().material.color = tile.baseColor;
                            //tile.centerPosition.SetActive(false);
                            tile.Disable();
                        }
                    }
                }
                GameManager.Instance.miniChessboard.possibleChessTiles.Add(tile);
            }
        }
    }
}
