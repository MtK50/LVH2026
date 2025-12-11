using NUnit.Framework;
using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class ChessboardManager : MonoBehaviour
{
    [SerializeField] public List<ChessTile> chessTiles = new List<ChessTile>();
    [SerializeField] public List<ChessPiece> chessPieces = new List<ChessPiece>();


    [SerializeField] public List<ChessTile> possibleChessTiles = new List<ChessTile>();
    [SerializeField] public Transform chessPiecesParent;

    private ChessPiece currentChessPiece;


    private void Start()
    {

        foreach(Transform child in transform)
        {
            ChessTile tile = child.GetComponent<ChessTile>();
            if (tile != null)
            {
                // This is a 5x5 chessboard, each tile is named as "SM_Tile*" (from 0 to 24);
                int index = int.Parse(child.name.Replace("SM_Tile", ""));
                int x = index % 5;
                int y = index / 5;
                tile.Initialize(x, y);
                chessTiles.Add(tile);
            }
        }
        foreach(Transform child in chessPiecesParent)
        {
            ChessPiece piece = child.GetComponent<ChessPiece>();
            if (piece != null)
            {
                piece.Initialize();
                chessPieces.Add(piece);
            }
        }
    }

    public void SetCurrentChessPiece(ChessPiece piece)
    {
        currentChessPiece = piece;
    }

    public void UpdateChessPiecePosition(ChessPiece piece)
    {
        if(piece.pieceName == "null")
        {
            return;
        }
        // The lowest distance tile of all chessTiles is considered the current tile of the piece
        float minDistance = float.MaxValue;
        ChessTile closestTile = null;
        foreach (ChessTile tile in chessTiles)
        {
            float distance = Vector3.Distance(piece.transform.position, tile.transform.position);
            if (distance < minDistance && tile.isSelectable)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }
        if (closestTile != null)
        {
            piece.boardPosition = closestTile.position;
            Debug.Log($"Chess Piece {piece.pieceName} is now on tile ({closestTile.position.x}, {closestTile.position.y})");


            if(closestTile.GetComponent<Renderer>().material.color == Color.red)
            {
                foreach(ChessPiece ennemy in chessPieces)
                {
                    if(ennemy.pieceTeam != piece.pieceTeam && ennemy.boardPosition == closestTile.position)
                    {
                        Debug.Log($"Chess Piece {piece.pieceName} captured {ennemy.pieceName}!");
                        ennemy.gameObject.SetActive(false);
                        // Optionally, remove the captured piece from the chessPieces list
                         chessPieces.Remove(ennemy);
                        break;
                    }
                }
            }
            StartCoroutine(GameManager.Instance.WaitAndNextTurn(2.0f));
        }
    }
}
