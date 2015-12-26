using UnityEngine;
using System.Collections;
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
    public delegate void PathCompleteCallback(Stack<MapTile> completePath);

    public int StartX;
    public int StartY;
    public int EndX;
    public int EndY;
    public PathCompleteCallback Callback;

    public PathingTask(int startX, int startY, int endX, int endY, PathCompleteCallback callback)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        Callback = callback;
    }
}

public class AStarPather : MonoBehaviour
{
    public Map Map { get; private set; }

    public Queue<PathingTask> tasks;

    public int Tasks = 0;
    public bool Ready = false;

    private void Awake()
    {
        tasks = new Queue<PathingTask>();
    }

    private void Start()
    {
        StartCoroutine(UpdatePathingTasks());
    }

    public void Init(Map map)
    {
        UpdateMap(map);        
    }

    public void UpdateMap(Map map)
    {
        Map = map;

        Ready = true;
    }

    public void AddTask(PathingTask newTask)
    {
        tasks.Enqueue(newTask);
    }

    private IEnumerator UpdatePathingTasks()
    {
        while (true)
        {
            if (tasks.Count == 0 || !Ready)
                yield return new WaitUntil(() => tasks.Count != 0 && Ready);

            while (tasks.Count != 0 && Ready)
            {
                var task = tasks.Dequeue();

                // solve pathing task
                

                // return completed path to caller
                var completePath = new Stack<MapTile>();
                task.Callback(completePath);
                yield return null;
            }
        }
    }

    public void Update()
    {
        if (!Ready)
            return;

        Tasks = tasks.Count;
    }
}
