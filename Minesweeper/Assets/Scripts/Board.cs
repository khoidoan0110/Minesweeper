using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Tile tileUnknown, tileEmpty, tileMine, tileExploded, tileFlag, tile1, tile2, tile3, tile4, tile5, tile6, tile7, tile8;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        //use [,] to tell that you are using array 2d
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    private Tile GetTile(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {
            return tileFlag;
        }
        else if (cell.emptied)
        {
            return tileEmpty;
        }
        else
        {
            return tileUnknown;
        }
    }

    private Tile GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty: return tileEmpty;
            case Cell.Type.Mine: return cell.exploded ? tileExploded : tileMine;
            case Cell.Type.Number: return GetNumberedTile(cell);
            default: return null;
        }
    }

    private Tile GetNumberedTile(Cell cell)
    {
        switch (cell.number)
        {
            case 1: return tile1;
            case 2: return tile2;
            case 3: return tile3;
            case 4: return tile4;
            case 5: return tile5;
            case 6: return tile6;
            case 7: return tile7;
            case 8: return tile8;
            default: return null;
        }
    }
}
