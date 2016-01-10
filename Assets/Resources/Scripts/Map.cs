using UnityEngine;
using Priority_Queue;
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

public enum Direction
{
    North,
    East,
    South,
    West
}

public class MapTile : FastPriorityQueueNode
{ 
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; }

    public MapTile(int x, int y, TileType tile)
    {
        X = x;
        Y = y;
        Type = tile;
    }
}

public class Map
{
    private MapTile[] tileMap;
    public int Size { get; private set; }

    public GameLogic Game;
    public GameObject TerrainObj;   // where the map creation and click events will happen, however handlers
                                    // will be passed to mapRep so all map logic happens in this class
    public TerrainRep TerrainRep;
    public GameObject MapLabels;

    // movement time to cross a tile (unit speed modifier)
    public readonly Dictionary<TileType, double> MovementCosts = new Dictionary<TileType, double>()
    {
        { TileType.Grass, 1d },
        { TileType.Road, 0.25d },
        { TileType.Trees, 3d },
    };

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
        GenerateMapLabels();
    }

    private void GenerateMapLabels()
    {
        MapLabels = new GameObject("Map Labels");
    }

    public void AddLabel(int x, int y, string label)
    {
        var updated = false;
        foreach (var existing in MapLabels.GetComponentsInChildren<MapLabel>())
        {
            if (existing.transform.position.x == x  + 0.5f && existing.transform.position.z == y + 0.5f)
            {
                existing.Label = label;
                updated = true;
                break;
            }
        }

        if (updated)
            return;

        var newLabel = new GameObject(label).AddComponent<MapLabel>();
        newLabel.transform.parent = MapLabels.transform;
        newLabel.Init(x, y, label);
    }

    public void DrawDebugPath(Queue<Vector2> path, float seconds, Color colour)
    {
        if (path.Count < 2)
            return;

        Vector2 prev = path.Dequeue();

        while (path.Count != 0)
        {
            Vector2 next = path.Dequeue();            
            Debug.DrawLine(new Vector3(prev.x + 0.5f, 0f, prev.y + 0.5f),
                           new Vector3(next.x + 0.5f, 0f, next.y + 0.5f),
                           colour,
                           seconds,
                           false);
            prev = next;
        }
    }

    private void GenerateTileMap()
    {
        var totalSize = Size * Size;
        tileMap = new MapTile[totalSize];

        // large grass field by default
        for (int i = 0; i < totalSize; i++)
            tileMap[i] = new MapTile(i % Size, i / Size, TileType.Grass);

        GenerateForests();
        GenerateRoads();
    }

    private void CreateTerrainRep()
    {
        TerrainObj = new GameObject("TerrainRep");

        TerrainRep = TerrainObj.AddComponent<TerrainRep>();
        TerrainRep.Generate(this);

        TerrainObj.transform.parent = Game.transform;
    }

    #region Forest generation

    private void GenerateForests()
    {
        int forestSeeds = 50;
        int maxGenerations = 500;

        for (int i = 0; i < forestSeeds; i++)
        {
            int seedX = Random.Range(2, Size - 2);
            int seedY = Random.Range(2, Size - 2);

            SetTileTypeAt(seedX, seedY, TileType.Trees);
            SetTileTypeAt(seedX - 1, seedY, TileType.Trees);
            SetTileTypeAt(seedX, seedY - 1, TileType.Trees);
            SetTileTypeAt(seedX + 1, seedY, TileType.Trees);
            SetTileTypeAt(seedX, seedY + 1, TileType.Trees);

            var openList = new List<Vector2>()
            {
                new Vector2(seedX - 1, seedY - 1),
                new Vector2(seedX + 1, seedY + 1),
                new Vector2(seedX - 1, seedY + 1),
                new Vector2(seedX + 1, seedY - 1),
            };

            for (int gen = 0; gen < maxGenerations; gen++)
            {
                if (openList.Count == 0)
                    break;
                
                var curr = openList[0];
                openList.RemoveAt(0);

                var currTile = GetTileTypeAt(curr);
                if (currTile == TileType.Trees || currTile == TileType.INVALID)
                    continue;

                int chance = 0;
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        if (GetTileTypeAt(curr + new Vector2(x, y)) == TileType.Trees)
                            chance += 20;

                var roll = Random.Range(0, 100);
                if (roll < chance)
                {
                    SetTileTypeAt(curr, TileType.Trees);
                    openList.Add(new Vector2(curr.x - 1, curr.y));
                    openList.Add(new Vector2(curr.x, curr.y - 1));
                    openList.Add(new Vector2(curr.x + 1, curr.y));
                    openList.Add(new Vector2(curr.x, curr.y + 1));
                }
            }
        }
    }

    #endregion

    #region Road generation

    private void GenerateRoads()
    {
        // 1:20%, 2:50%, 3:20%, 4:10%
        var straightDistanceOdds = new int[10] { 1, 1, 2, 2, 2, 2, 2, 3, 3, 4 };

        var turnDistanceOdds = new int[50] {
            -4,
            -4, -4,
            -3, -3, -3, 
            -2, -2, -2, -2,
            -1, -1, -1, -1, -1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,            
            1, 1, 1, 1, 1,
            2, 2, 2, 2,
            3, 3, 3,
            4, 4,
            4
        };

        var directionsLeft = new List<Direction>() { 
            Direction.North, Direction.East, Direction.South, Direction.West 
        };

        // two legs of roads, from centre to 2 edges
        for (int leg = 0; leg < 4; leg++)
        {
            Direction legDirection = directionsLeft[Random.Range(0, directionsLeft.Count)];
            directionsLeft.Remove(legDirection);

            int mid = Size / 2;
            int x = mid;
            int y = mid;

            while (GetTileTypeAt(x, y) != TileType.INVALID)
            {
                var straightDistance = straightDistanceOdds[Random.Range(0, 10)];
                var turnDistance = turnDistanceOdds[Random.Range(0, 50)];

                PaintRoad(ref x, ref y, legDirection, straightDistance, turnDistance);
            }
        }
    }
    
    private void PaintRoad(ref int x, ref int y, Direction direction, int strDst, int turnDst)
    {
        int oldX = x;
        int oldY = y;

        if (direction == Direction.North)
        {
            x += turnDst;
            y += strDst;
        }
        else if (direction == Direction.South)
        {
            x += turnDst;
            y -= strDst;
        }
        else if (direction == Direction.West)
        {
            x -= strDst;
            y += turnDst;

        }
        else if (direction == Direction.East)
        {
            x += strDst;
            y += turnDst;
        }

        // paint two lines for a road leg, one vertical one horizontal
        PaintRoadLine(oldX, oldY, oldX, y);
        PaintRoadLine(oldX, y, x, y);
    }
    
    private void PaintRoadLine(int x1, int y1, int x2, int y2)
    {
        // vertical line drawing
        if (x1 == x2)
        {
            if (y1 > y2)
                Util.Swap(ref y1, ref y2);

            for (int y = y1; y <= y2; y++)
                SetTileTypeAt(x1, y, TileType.Road);
        }
        else // horizontal
        {
            if (x1 > x2)
                Util.Swap(ref x1, ref x2);

            for (int x = x1; x <= x2; x++)
                SetTileTypeAt(x, y1, TileType.Road);
        }
    }
    
    #endregion

    #region Accessors / Utils

    public void SetTileTypeAt(Vector2 tilePos, TileType tileType)
    {
        SetTileTypeAt((int)tilePos.x, (int)tilePos.y, tileType);
    }

    private void SetTileTypeAt(int x, int y, TileType tileType)
    {
        if (x >= 0 && x < Size &&
            y >= 0 && y < Size)
            tileMap[x + y * Size].Type = tileType;
    }

    public TileType GetTileTypeAt(Vector2 tilePos)
    {
        return GetTileTypeAt((int)tilePos.x, (int)tilePos.y);
    }

    public TileType GetTileTypeAt(int x, int y)
    {
        if (x >= 0 && x < Size &&
            y >= 0 && y < Size)
            return tileMap[x + y * Size].Type;

        return TileType.INVALID;
    }

    public MapTile GetTileAt(Vector2 nodePos)
    {
        return GetTileAt((int)nodePos.x, (int)nodePos.y);
    }

    public MapTile GetTileAt(int x, int y)
    {
        if (x >= 0 && x < Size &&
            y >= 0 && y < Size)
            return tileMap[x + y * Size];

        return null;
    }

    public Vector2 WorldToTilePos(Vector3 worldPos)
    {
        return new Vector2(Mathf.FloorToInt(worldPos.x),
                           Mathf.FloorToInt(worldPos.z));
    }

    #endregion

}
