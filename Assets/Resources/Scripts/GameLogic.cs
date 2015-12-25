using UnityEngine;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    private Map map;
    public int MapSize = 128;

    public float TicksPerSecond = 3;
    public uint GameTick = 0;

    private float lastTick = 0f;
    private float timeBetweenTicks = 0.333f;

    // Use this for initialization
    void Start()
    {
        map = new Map(this, MapSize);

        lastTick = Time.time;
        timeBetweenTicks = 1f / TicksPerSecond;
    }

    // Update is called once per frame
    void Update()
    {
        // 3 ticks a second
        if (Time.time - lastTick > timeBetweenTicks)
        {
            lastTick = Time.time;
            OnTick();
        }
    }

    private void OnTick()
    {
        GameTick++;


    }
}
