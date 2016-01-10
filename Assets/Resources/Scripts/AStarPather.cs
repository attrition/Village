﻿using UnityEngine;
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
                
                var open = new FastPriorityQueue<MapTile>(2048*2048);
                var costSoFar = new Dictionary<MapTile, double>();

                var minCost = task.Unit.Details.Speed * Map.MovementCosts[TileType.Road];

                // add starting tile to open list
                var start = Map.GetTileAt(task.StartX, task.StartY);
                var goal = Map.GetTileAt(task.EndX, task.EndY);

                open.Enqueue(start, 0);
                costSoFar.Add(start, 0);

                // path to goal
                while (true)
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
                        Map.GetTileAt(  node.X,       node.Y + 1  ),
                        Map.GetTileAt(  node.X,       node.Y - 1  ),
                        Map.GetTileAt(  node.X + 1,   node.Y      ),
                    };

                    foreach (var neighbour in neighbours)
                    {
                        if (neighbour == null || costSoFar.ContainsKey(neighbour))
                            continue;
                        
                        var movecost = (task.Unit.Details.Speed * Map.MovementCosts[neighbour.Type]);
                        var g = costSoFar[node] + movecost;

                        // if not in open or is in open and new path is faster
                        if (!open.Contains(neighbour) || 
                            (costSoFar.ContainsKey(neighbour) && g < costSoFar[neighbour]))
                        {
                            // f = g + h
                            double h = (ManhattanHeuristic(neighbour, goal, minCost) * (1.001d));
                            double f = g + h;

                            // we keep track of g (cost to travel to node) but examine them in order of f (g + heuristic)
                            costSoFar[neighbour] = g;
                            if (open.Contains(neighbour))
                                open.UpdatePriority(neighbour, f);
                            else
                                open.Enqueue(neighbour, f);
                            
                            Map.AddLabel(neighbour.X, neighbour.Y, ((int)f).ToString());
                        }

                        //Debug.DrawLine(new Vector3(x + 0.5f, 0f, y + 0.5f), new Vector3(xN + 0.5f, 0f, yN + 0.5f), Color.red, 15f);
                    }

                    yield return null;
                }                

                // return completed path to caller
                var completePath = new Stack<MapTile>();
                if (task.Callback != null)
                    task.Callback(completePath);
                yield return null;
            }
        }
    }

    private double ManhattanHeuristic(MapTile a, MapTile b, double d)
    {
        return d * (Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y));
    }

    public void Update()
    {
        if (!Ready)
            return;

        Tasks = tasks.Count;
    }
}
