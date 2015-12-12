using UnityEngine;
using System.Collections.Generic;

public class Map
{
    public enum TileType
    {
        Grass,
        Road,
        Trees,
        Rock,
        Water,
        Dirt,
        Sand,
        INVALID,
    }

    // belongs in map rep
    Dictionary<TileType, Color> TileColour = new Dictionary<TileType, Color>()
    {
        { TileType.Grass, new Color(0, 102, 34) },
        { TileType.Road, new Color(173, 114, 57) },
        { TileType.Trees, new Color(0, 51, 0) },
        { TileType.Rock, new Color(230, 230, 230) },
        { TileType.Water, new Color(51, 51, 255) },
        { TileType.Dirt, new Color(102, 51, 0) },
        { TileType.Sand, new Color(255, 255, 204) },
    };

    TileType[] tileMap;
    public int Size { get; private set; }

    //GameObject mapRep; // where the map creation and click events will happen, however handlers
    // will be passed to mapRep so all map logic happens in this class

    public Map(int size)
    {
        this.Size = size;
        GenerateMap();
    }

    private void GenerateMap()
    {
        var totalSize = Size * Size;
        tileMap = new TileType[totalSize];

        for (int i = 0; i < totalSize; i++)
            tileMap[i] = TileType.Grass;
    }

    public TileType GetTileTypeAt(int x, int y)
    {
        if (x >= 0 && x < Size &&
            y >= 0 && y < Size)
            return tileMap[x + y * Size];

        return TileType.INVALID;
    }
}
