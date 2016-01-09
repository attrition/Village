using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

public struct PathingTask
{
    public delegate void PathCompleteCallback(Stack<MapTile> completePath);

    public Unit Unit;
    public int StartX;
    public int StartY;
    public int EndX;
    public int EndY;
    public PathCompleteCallback Callback;

    public PathingTask(Unit unit, int startX, int startY, int endX, int endY, PathCompleteCallback callback)
    {
        Unit = unit;
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

            // go through task queue
            while (tasks.Count != 0 && Ready)
            {
                // solve pathing task
                var task = tasks.Dequeue();
                bool done = false;

                // worst case handling of 1024*1024
                var open = new HeapPriorityQueue<MapTile>(1048576);
                var closed = new Dictionary<MapTile, double>();
                var openCosts = new Dictionary<MapTile, double>();

                // add starting tile to open list
                double cost = ManhattanDistance(task.StartX, task.StartY, task.EndX, task.EndY);
                var startTile = Map.GetTileAt(task.StartX, task.StartY);
                open.Enqueue(startTile, cost); // g + h
                openCosts.Add(startTile, cost); // g FIX THIS

                // path to goal
                while (!done)
                {
                    if (open.Count == 0)
                        break;                    

                    var node = open.Dequeue();
                    closed.Add(node, cost);
                    
                    // rewind path if found
                    if (node.X == task.EndX && node.Y == task.EndY)
                        break;

                    int x = node.X;
                    int y = node.Y;

                    // add/inspect neighbours
                    for (int xN = x - 1; xN <= x + 1; xN++)
                    {
                        for (int yN = y - 1; yN <= y + 1; yN++)
                        {
                            // skip current tile
                            if (xN == x && yN == y)
                                continue;

                            // out of bounds check
                            if (xN >= Map.Size || yN >= Map.Size || xN < 0 || yN < 0)
                                continue;

                            var neighbour = Map.GetTileAt(xN, yN);

                            // move = old cost + movement cost to neighbour
                            var movecost = openCosts[node] + (task.Unit.Details.Speed * Map.MovementCosts[neighbour.Type]);

                            double neighbourcost = double.MaxValue;
                            if (openCosts.ContainsKey(neighbour))
                                neighbourcost = openCosts[neighbour];

                            // if new path is better remove neighbour from open list
                            if (open.Contains(neighbour) && movecost < neighbourcost)
                                open.Remove(neighbour);
                            else if (closed.ContainsKey(neighbour) && movecost < neighbourcost)
                                closed.Remove(neighbour);
                            else if (!open.Contains(neighbour) && !closed.ContainsKey(neighbour))
                            {
                                double newcost = movecost + ManhattanDistance(xN, yN, task.EndX, task.EndY);
                                openCosts.Add(neighbour, movecost);
                                open.Enqueue(neighbour, newcost);
                            }

                            Debug.DrawLine(new Vector3(x + 0.5f, 0f, y + 0.5f), new Vector3(xN + 0.5f, 0f, yN + 0.5f), Color.red, 15f);
                        }
                    }

                    //yield return null;
                }                

                // return completed path to caller
                var completePath = new Stack<MapTile>();
                if (task.Callback != null)
                    task.Callback(completePath);
                yield return null;
            }
        }
    }

    private double ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    public void Update()
    {
        if (!Ready)
            return;

        Tasks = tasks.Count;
    }
}
