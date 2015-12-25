using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;

public class AStarMapNode : PriorityQueueNode
{
    public MapTile Tile { get; private set; }
    public int Cost { get; set; }

    public AStarMapNode(MapTile tile, int cost)
    {
        Tile = tile;
        Cost = cost;
    }
}

public struct PathingTask
{
    public int StartX;
    public int StartY;
    public int EndX;
    public int EndY;

    public PathingTask(int startX, int startY, int endX, int endY)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
    }
}

public class AStarPather : MonoBehaviour
{
    public Map Map { get; private set; }

    public Queue<PathingTask> complete;
    public Queue<PathingTask> working;

    public AStarPather(Map map)
    {
        complete = new Queue<PathingTask>();
        working = new Queue<PathingTask>();

        UpdateMap(map);
    }

    public void UpdateMap(Map map)
    {
        Map = map;
    }

    public void AddTask(PathingTask newTask)
    {
        working.Enqueue(newTask);
    }

    public void Update()
    {

    }
}
