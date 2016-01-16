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
    public long TimeQueued;

    public PathingTask(Unit unit, int startX, int startY, int endX, int endY, PathCompleteCallback callback)
    {
        Unit = unit;
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        Callback = callback;
        TimeQueued = System.DateTime.Now.Ticks;
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
        while (Ready)
        {
            if (tasks.Count == 0 || !Ready)
                yield return new WaitUntil(() => tasks.Count != 0 && Ready);

            // go through task queue
            while (tasks.Count != 0 && Ready)
            {
                // solve pathing task
                var task = tasks.Dequeue();
                long frameTime = System.DateTime.Now.Ticks;

                var open = new FastPriorityQueue<MapTile>(Map.Size * Map.Size);
                var costSoFar = new Dictionary<MapTile, double>();

                var minCost = task.Unit.Details.Speed * Map.MovementCosts[TileType.Road];

                // add starting tile to open list
                var start = Map.GetTileAt(task.StartX, task.StartY);
                var goal = Map.GetTileAt(task.EndX, task.EndY);
                
                open.Enqueue(start, 0);
                costSoFar[start] = 0;

                // path to goal
                while (Ready)
                {
                    if (open.Count == 0)
                        break;

                    var node = open.Dequeue();

                    // rewind path if found
                    if (node == goal)
                        break;
                    
                    // add/inspect neighbours
                    var neighbours = new List<MapTile>()
                    {
                        Map.GetTileAt(  node.X - 1,   node.Y      ),
                        Map.GetTileAt(  node.X + 1,   node.Y      ),
                        Map.GetTileAt(  node.X,       node.Y + 1  ),
                        Map.GetTileAt(  node.X,       node.Y - 1  ),
                    };

                    foreach (var neighbour in neighbours)
                    {
                        if (!Ready)
                            break;

                        if (neighbour == null || costSoFar.ContainsKey(neighbour))
                            continue;

                        var movecost = Map.GetMovementCostAt(neighbour, task.Unit);
                        var g = costSoFar[node] + movecost;

                        // if not in open or is in open and new path is faster
                        if (!open.Contains(neighbour) || 
                            (costSoFar.ContainsKey(neighbour) && g < costSoFar[neighbour]))
                        {
                            // f = g + h
                            double h = ManhattanHeuristic(start, neighbour, goal, minCost);
                            double f = g + h;

                            // we keep track of g (cost to travel to node) but examine them in order of f (g + heuristic)
                            costSoFar[neighbour] = g;
                            if (open.Contains(neighbour))
                                open.UpdatePriority(neighbour, f);
                            else
                                open.Enqueue(neighbour, f);
                            
                            Map.AddLabel(neighbour.X, neighbour.Y, ((int)g).ToString());
                        }
                    }

                    // 16ms allocated per frame
                    if ((System.DateTime.Now.Ticks - frameTime) / 10000L > 16L)
                    {
                        //Debug.Log("yielding: " + (System.DateTime.Now.Ticks - frameTime) / 10000l);
                        frameTime = System.DateTime.Now.Ticks;
                        yield return null;
                    }
                }

                // return completed path to caller
                var completePath = new Stack<MapTile>();

                MapTile lowest = goal;
                MapTile curr = goal;

                while (curr != start && Ready)
                {
                    completePath.Push(curr);

                    //TODO: prefer to check tiles on the slope between start->goal
                    var neighbours = new List<MapTile>()
                    {
                        Map.GetTileAt(curr.X - 1, curr.Y),
                        Map.GetTileAt(curr.X + 1, curr.Y),
                        Map.GetTileAt(curr.X, curr.Y - 1),
                        Map.GetTileAt(curr.X, curr.Y + 1),
                    };

                    var lowestCost = costSoFar[curr];
                    foreach (var neighbour in neighbours)
                    {
                        if (!Ready)
                            break;

                        if (neighbour == null || !costSoFar.ContainsKey(neighbour))
                            continue;

                        if (costSoFar[neighbour] < lowestCost)
                        {
                            lowestCost = costSoFar[neighbour];
                            lowest = neighbour;
                        }
                    }

                    curr = lowest;                    
                }            

                //Debug.Log("Pathing complete in: " + (System.DateTime.Now.Ticks - task.TimeQueued) / 10000L);
                if (task.Callback != null)
                    task.Callback(completePath);

                yield return null;
            }
        }
    }

    private double ManhattanHeuristic(MapTile start, MapTile current, MapTile goal, double d)
    {
        var dx1 = current.X - goal.X;
        var dy1 = current.Y - goal.Y;
        var dx2 = start.X - goal.X;
        var dy2 = start.Y - goal.Y;
        var cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
        var heuristic = (d * 2d) * (Mathf.Abs(current.X - goal.X) + Mathf.Abs(current.Y - goal.Y));
        heuristic += cross * 0.001d;
        heuristic *= 1.101d;

        return heuristic;
    }

    public void Update()
    {
        if (!Ready)
            return;

        Tasks = tasks.Count;
    }
}
