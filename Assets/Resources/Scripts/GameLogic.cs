using UnityEngine;
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

    private float ticksPerSecond = 8;
    private float timeBetweenTicks = 0.125f;
    private float lastTick = 0f;

    void Awake()
    {
        if (GameSeed == 0)
            GameSeed = System.Guid.NewGuid().GetHashCode();

        Random.seed = GameSeed;
    }

    // Use this for initialization
    void Start()
    {
        Map = new Map(this, MapSize);
        Pathfinder = gameObject.AddComponent<AStarPather>();
        Pathfinder.UpdateMap(Map);

        lastTick = Time.time;
        timeBetweenTicks = 1f / ticksPerSecond;

        UnitObjects = new GameObject("Units");
        for (int i = 0; i < 10; i++)
        {
            var x = Random.Range(0, Map.Size);
            var y = Random.Range(0, Map.Size);
            var type = Map.GetTileTypeAt(x, y);

            if (type == TileType.Grass || type == TileType.Road)
            {
                var unit = UnitObjectFactory.MakeVillager(this, x, y);
                Units.Add(unit);

                unit.transform.parent = UnitObjects.transform;
            }
        }

        var u = Units[0].GetComponent<Unit>();
        Pathfinder.AddTask(new PathingTask(u, u.X, u.Y, 64, 64, DebugCallbackComplete));
        Units.Add(UnitObjectFactory.MakeVillager(this, 64, 64));
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
    }

    private void DebugCallbackComplete(Stack<MapTile> completePath)
    {
        Debug.Log("Completed debug path");
        Map.DrawDebugPath(completePath, 30f, Color.blue);
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
