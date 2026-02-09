using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece prefabPiece;
    public Tilemap tilemap;
    public TetronimoData[] tetronimos;
    public Vector2Int boardSize;
    public Vector2Int startPosition = new Vector2Int(-1, 8);

    private Piece activePiece;

    private float dropTime = 0.0f;
    private float dropInterval = 0.5f;

    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece> ();

    Tetronimo[] setPieces = new Tetronimo [] { Tetronimo.I, Tetronimo.I, Tetronimo.I, Tetronimo.G, Tetronimo.O, Tetronimo.L, Tetronimo.T};
    int setPiecesIndex = 0;

    int left
    {
        get { return -boardSize.x / 2; }
    }

    int right
    {
        get { return boardSize.x / 2; }
    }

    int top
    {
        get { return boardSize.y / 2; }
    }

    int bottom
    {
        get { return -boardSize.y / 2; }
    }
    
    private void Update()
    {
        if (tetrisManager.gameOver) return;

        dropTime += Time.deltaTime;

        if (dropTime >= dropInterval)
        {
            dropTime = 0.0f;

            Clear(activePiece);
            
            bool moveResult = activePiece.Move(Vector2Int.down);
            
            Set(activePiece);


            if (!moveResult)
            {
                activePiece.freeze = true;
                CheckBoard();
                SpawnPiece();
            }
        }
    }
    
    public void SpawnPiece()
    {
        activePiece = Instantiate(prefabPiece);

        Tetronimo t = Tetronimo.T;

        if(setPiecesIndex != setPieces.Length)
        {
            t = setPieces[setPiecesIndex];
        }
        else
        {
            startPosition = new Vector2Int(-1, 12);
            setPiecesIndex = 0;
        }

        setPiecesIndex++;
        activePiece.Initialize(this, t); 

        CheckEndGame();
        
        Set(activePiece);
    }


    public void UpdateGameOver()
    {
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    private void ResetBoard()
    {
        startPosition = new Vector2Int(-1, 8);
        setPiecesIndex = 0;
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (Piece piece in foundPieces)
        {
            Destroy(piece.gameObject);
        }
        activePiece = null;
        
        pieces.Clear();
        
        SpawnPiece();
    }

    private void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            tilemap.SetTile(cellPosition, null);
            pieces.Remove(cellPosition);
        }
        else
        {
            tilemap.SetTile(cellPosition, piece.data.tile);
            pieces[cellPosition] = piece;
        }
    }

    public void Clear(Piece piece)
    {

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, piece);
        }
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }
        return true;
    }

    void DestroyLine(int y)
    {

        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            if (pieces.ContainsKey(cellPosition))
            {
                Piece piece = pieces[cellPosition];

                piece.ReduceActiveCount();

                SetTile(cellPosition, null);
            }
            tilemap.SetTile(cellPosition, null);
        }
    }

    void ShiftRowsDown(int clearedRow)
    {

        for (int y = clearedRow + 1; y < top; y++)
        {

            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                if (pieces.ContainsKey(cellPosition))
                {               
                    Piece currentPiece = pieces[cellPosition];

               
                    SetTile(cellPosition, null);

                    cellPosition.y--;

                    SetTile(cellPosition, currentPiece);
                }

            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();

        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }
        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);
    }
    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        for (int i = 0; i < piece.cells.Length;  i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top)
                return false;

            if (tilemap.HasTile(cellPosition))
            {
                return false;
            }
        }

        return true;
    }

    private void CheckEndGame()
    {
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }

}
