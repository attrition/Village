﻿using UnityEngine;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    public Map Map { get; private set; }
    public AStarPather Pathfinder { get; private set; }

    public GameObject UnitObjects;
    public List<GameObject> Units;

    public uint GameTick = 0;

    public int MapSize = 128;
    public int GameSeed = 0;

    private float ticksPerSecond = 10f;
    private float timeBetweenTicks = 0.100f;
    private float lastTick = 0f;

    void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        NewGame(GameSeed == 0);
    }

    private void NewGame(bool newSeed)
    {
        if (newSeed)
            GameSeed = System.Guid.NewGuid().GetHashCode();

        Random.seed = GameSeed;

        Destroy(GameObject.Find("Trees"));
        Destroy(GameObject.Find("Map Labels"));
        Destroy(GameObject.Find("Units"));
        Destroy(GameObject.Find("TerrainRep"));

        Map = new Map(this, MapSize);

        Destroy(gameObject.GetComponent<AStarPather>());
        Pathfinder = gameObject.AddComponent<AStarPather>();
        Pathfinder.UpdateMap(Map);

        lastTick = Time.time;
        timeBetweenTicks = 1f / ticksPerSecond;

        Units = new List<GameObject>();
        UnitObjects = new GameObject("Units");
    }

    private GameObject AddVilligar(int xIn = -1, int yIn = -1)
    {
        int x = xIn;
        int y = yIn;

        if (x == -1 || y == -1)
        {
            while (true)
            {
                x = Random.Range(0, Map.Size);
                y = Random.Range(0, Map.Size);
                var type = Map.GetTileTypeAt(x, y);

                if (type == TileType.Grass || type == TileType.Road)
                    break;
            }
        }

        var unit = UnitObjectFactory.MakeVillager(this, x, y);
        Units.Add(unit);

        unit.transform.parent = UnitObjects.transform;
        return unit;
    }

    // Update is called once per frame
    void Update()
    {
        // 8 ticks a second
        if (Time.time - lastTick > timeBetweenTicks)
        {
            lastTick = Time.time;
            OnTick();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var u = AddVilligar().GetComponent<Unit>();
            Pathfinder.AddTask(new PathingTask(u, u.X, u.Y, Map.Size / 2, Map.Size / 2, u.PathingComplete));
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            var u = AddVilligar(0, 0).GetComponent<Unit>();
            Pathfinder.AddTask(new PathingTask(u, u.X, u.Y, Map.Size - 1, Map.Size - 1, u.PathingComplete));
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < 200; i++)
            {
                var u = AddVilligar().GetComponent<Unit>();
                Pathfinder.AddTask(new PathingTask(u, u.X, u.Y, Map.Size / 2, Map.Size / 2, u.PathingComplete));
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
            NewGame(true);

    }

    private void DebugCallbackComplete(Stack<MapTile> completePath)
    {
        Debug.Log("Completed debug path");
        Map.DrawDebugPath(completePath, 15f, Color.magenta);
    }

    private void OnTick()
    {
        GameTick++;

        // execute unit tick() functions
        foreach (var u in Units)
        {
            u.GetComponent<Unit>().OnTick();
        }
    }
}
