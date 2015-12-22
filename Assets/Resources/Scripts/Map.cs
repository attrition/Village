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

public enum Direction {
	North,
	East,
	South,
	West
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
		GenerateRoads();
    }

	private void GenerateRoads()
	{
		// 1:20%, 2:50%, 3:20%, 4:10%
		var straightDistanceOdds = new int[10] { 1, 1, 2, 2, 2, 2, 2, 3, 3, 4 };

		// Left:0:20%, None:1:50%, Right:2:20%
		var turnOdds = new int[5] { 0, 1, 1, 1, 2 };

		// 1:60%, 2:30%, 3:10%
		var turnDistanceOdds = new int[10] { 1, 1, 1, 1, 1, 1, 2, 2, 2, 3 };

		var directionsLeft = new List<Direction>() { 
			Direction.North, Direction.East, Direction.South, Direction.West 
		};

		// two legs of roads, from centre to 2 edges
		for (int leg = 0; leg < 4; leg++)
		{
			Direction legDirection = directionsLeft[Random.Range(0, directionsLeft.Count - 1)];
			directionsLeft.Remove(legDirection);

			int mid = Size / 2;
			int x = mid;
			int y = mid;

			while (GetTileTypeAt(x, y) != TileType.INVALID)
			{
				CreateRoadLeg(ref x, ref y, legDirection, straightDistanceOdds, turnOdds, turnDistanceOdds);
			}
		}
	}

	private void CreateRoadLeg(ref int x, ref int y, Direction direction, 
		                       int[] strDst, int[] turn, int[] turnDst)
	{
		var straightDistance = strDst[Random.Range(0, 9)];
		var turnCheck = turn[Random.Range(0, 4)];
		var turnDistance = turnDst[Random.Range(0, 9)];

		PaintRoad(ref x, ref y, direction, straightDistance, turnCheck, turnDistance);
	}

	private void PaintRoad(ref int x, ref int y, Direction direction, int strDst, int turn, int turnDst)
	{
		int oldX = x;
		int oldY = y;

		if (direction == Direction.North)
		{
			x = GetRoadTurnEnd(x, direction, turn, turnDst);
			y += strDst;
		}
		else if (direction == Direction.South)
		{
			x = GetRoadTurnEnd(x, direction, turn, turnDst);
			y -= strDst;
		}
		else if (direction == Direction.West)
		{
			x -= strDst;
			y = GetRoadTurnEnd(y, direction, turn, turnDst);
		}
		else if (direction == Direction.East)
		{
			x += strDst;
			y = GetRoadTurnEnd(y, direction, turn, turnDst);
		}

		PaintRoadFrom(oldX, oldY, x, y, direction);
	}

	private int GetRoadTurnEnd(int pos, Direction direction, int turn, int turnDst)
	{
		if (turn == 1)
			return pos;
		
		if (direction == Direction.North || direction == Direction.East)
			turn = (turn == 0) ? 2 : 0;

		pos += (turn == 0) ? -turnDst : turnDst;
		return pos;
	}

	private void PaintRoadFrom(int x1, int y1, int x2, int y2, Direction direction)
	{
		// FIXME: roads need to be painted with direction awareness

		if (x1 > x2)
			Util.Swap(ref x1, ref x2);
		if (y1 > y2)
			Util.Swap(ref y1, ref y2);

		for (int x = x1; x < x2; x++)
		{
			if (GetTileTypeAt(x, y1) == TileType.INVALID)
				continue;
			tileMap[x + y1 * Size] = TileType.Road;
		}

		for (int y = y1; y < y2; y++)
		{
			if (GetTileTypeAt(x2, y) == TileType.INVALID)
				continue;
			tileMap[x2 + y * Size] = TileType.Road; 
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
