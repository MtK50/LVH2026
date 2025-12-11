using Meta.Voice.Net.WebSockets;
using Oculus.VoiceSDK.UX;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    [SerializeField] public GameObject player;
    [SerializeField] public ChessboardManager miniChessboard;



    private ChessPiece lastSelectedPiece = null;
    private bool isBlueTurn = true;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MakePionPlayMove()
    {
        ChessPiece selectedPion = miniChessboard.chessPieces[Random.Range(0, miniChessboard.chessPieces.Count)];
        if(selectedPion == null || selectedPion.pieceName == "null") { return; }

        if(lastSelectedPiece != null )
        {
            if ((lastSelectedPiece == selectedPion || lastSelectedPiece?.pieceTeam == selectedPion.pieceTeam) 
                && miniChessboard.chessPieces.Count != 1)
            {
                MakePionPlayMove();
                return;
            }
        }
        
        foreach(ChessTile tile in miniChessboard.chessTiles)
        {
            tile.GetComponent<Renderer>().material.color = tile.baseColor;
            tile.Disable();

        }

        foreach (ChessPiece piece in miniChessboard.chessPieces)
        {
            if(piece.pieceName == selectedPion.pieceName)
            {
                piece.PionTurn(true);
                lastSelectedPiece = piece;
            }
            else
            {
                piece.PionTurn(false);
            }
        }
    }


    public IEnumerator WaitAndNextTurn(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        foreach (ChessPiece piece in miniChessboard.chessPieces)
        {
            piece.piece4DS.Play(false);
        }
        isBlueTurn = !isBlueTurn;
        MakePionPlayMove();
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            MakePionPlayMove();
        }   

    }




}
