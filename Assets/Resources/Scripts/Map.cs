using UnityEngine;
using System.Collections.Generic;

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

public class Map
{
    private TileType[] tileMap;
    public int Size { get; private set; }

    public GameLogic Game;
    public GameObject TerrainObj;   // where the map creation and click events will happen, however handlers
                                    // will be passed to mapRep so all map logic happens in this class
    public TerrainRep TerrainRep;

    public Map(GameLogic game, int size)
    {
        this.Game = game;
        Generate(size);
    }

    public void Generate(int size)
    {
        this.Size = size;

        GenerateTileMap();
        CreateTerrainRep();
    }

    private void GenerateTileMap()
    {
        var totalSize = Size * Size;
        tileMap = new TileType[totalSize];

        // default grass field
        for (int i = 0; i < totalSize; i++)
            tileMap[i] = TileType.Grass;

        // drunk walk some roads center-to-east
        int mid = Size / 2;
        int y = mid;

        for (int x = mid; x < Size; x++)
        {
            var tileIdx = x + y * Size;
            var tile = TileType.Road;
            tileMap[tileIdx] = tile;
        }

        y = mid;
        for (int x = mid; x >= 0; x--)
        {
            var tileIdx = x + y * Size;
            var tile = TileType.Road;
            tileMap[tileIdx] = tile;
        }
    }

    private void CreateTerrainRep()
    {
        TerrainObj = new GameObject("TerrainRep");

        TerrainRep = TerrainObj.AddComponent<TerrainRep>();
        TerrainRep.Generate(this);

        TerrainObj.transform.parent = Game.transform;
    }

    public TileType GetTileTypeAt(Vector2 tilePos)
    {
        return GetTileTypeAt((int)tilePos.x, (int)tilePos.y);
    }

    public TileType GetTileTypeAt(int x, int y)
    {
        if (x >= 0 && x < Size &&
            y >= 0 && y < Size)
            return tileMap[x + y * Size];

        return TileType.INVALID;
    }

    public Vector2 WorldToTilePos(Vector3 worldPos)
    {
        return new Vector3(Mathf.FloorToInt(worldPos.x),
                           Mathf.FloorToInt(worldPos.z));
    }
}
