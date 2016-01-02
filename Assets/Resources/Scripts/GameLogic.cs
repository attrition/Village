using UnityEngine;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    public Map Map { get; private set; }
    public AStarPather Pathfinder { get; private set; }

    public GameObject UnitObjects;
    public List<GameObject> Units;

    public int MapSize = 128;

    public uint GameTick = 0;

    private float ticksPerSecond = 8;
    private float timeBetweenTicks = 0.125f;
    private float lastTick = 0f;

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
