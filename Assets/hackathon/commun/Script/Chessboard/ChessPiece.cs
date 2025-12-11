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
    [Header("Chess Piece Settings")]
    public Vector2 boardPosition;


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
        pieceName = gameObject.name;

        piece4DS = GetComponentInChildren<unity4dv.Plugin4DS>();
        Debug.Log(piece4DS.SequenceName);
    }



}
