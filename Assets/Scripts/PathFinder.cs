using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MapGen;

public class Node
{
    public Node cameFrom = null; // Parent node
    public double priority = 0; // F value
    public double costSoFar = 0; // G value
    public Tile tile;

    public Node(Tile _tile, double _priority, Node _cameFrom, double _costSoFar)
    {
        cameFrom = _cameFrom;
        priority = _priority;
        costSoFar = _costSoFar;
        tile = _tile;
    }
}

public class PathFinder
{
    List<Node> TODOList = new List<Node>();
    List<Node> DoneList = new List<Node>();
    Tile goalTile;
    List<Enemy> enemies;

    // Constructor
    public PathFinder(List<Enemy> _enemies)
    {
        goalTile = null;
        enemies = _enemies; // Initialize with the list of enemies
    }

    
    public Queue<Tile> FindPathAStar(Tile start, Tile goal)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();

        TODOList.Add(new Node(start, 0, null, 0)); 
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => x.priority.CompareTo(y.priority)); 
            Node current = TODOList[0];
            DoneList.Add(current);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current); 
            }

            foreach (Tile nextTile in current.tile.Adjacents)
            {
                if (DoneList.Exists(node => node.tile == nextTile))
                    continue;

                // Calculate costs
                double newCostToNeighbor = current.costSoFar + 10; 
                double priority = newCostToNeighbor + HeuristicsDistance(nextTile, goal);

                Node neighborNode = TODOList.Find(node => node.tile == nextTile);
                if (neighborNode == null)
                {
                    neighborNode = new Node(nextTile, priority, current, newCostToNeighbor);
                    TODOList.Add(neighborNode);
                }
                else if (newCostToNeighbor < neighborNode.costSoFar)
                {
                    neighborNode.costSoFar = newCostToNeighbor;
                    neighborNode.priority = priority;
                    neighborNode.cameFrom = current;
                }
            }
        }
        return new Queue<Tile>(); 
    }

    // A* Algorithm with enemy evasion 
    public Queue<Tile> FindPathAStarEvadeEnemy(Tile start, Tile goal)
    {
        TODOList = new List<Node>();
        DoneList = new List<Node>();

        TODOList.Add(new Node(start, 0, null, 0)); 
        goalTile = goal;

        while (TODOList.Count > 0)
        {
            TODOList.Sort((x, y) => x.priority.CompareTo(y.priority)); 
            Node current = TODOList[0];
            DoneList.Add(current);
            TODOList.RemoveAt(0);

            if (current.tile == goal)
            {
                return RetracePath(current); 
            }

            foreach (Tile nextTile in current.tile.Adjacents)
            {
                if (DoneList.Exists(node => node.tile == nextTile))
                    continue;

                
                double penalty = IsTileNearEnemy(nextTile) ? 30 : 0; 
                double newCostToNeighbor = current.costSoFar + 10 + penalty; 
                double priority = newCostToNeighbor + HeuristicsDistance(nextTile, goal);

                Node neighborNode = TODOList.Find(node => node.tile == nextTile);
                if (neighborNode == null)
                {
                    neighborNode = new Node(nextTile, priority, current, newCostToNeighbor);
                    TODOList.Add(neighborNode);
                }
                else if (newCostToNeighbor < neighborNode.costSoFar)
                {
                    neighborNode.costSoFar = newCostToNeighbor;
                    neighborNode.priority = priority;
                    neighborNode.cameFrom = current;
                }
            }
        }
        return new Queue<Tile>(); 
    }

    // Check if the tile is near an enemy (used for evasion)
    private bool IsTileNearEnemy(Tile tile)
    {
        if (enemies == null || enemies.Count == 0)
    {
        return false;
    }

        float threshold = 5.0f; 
        foreach (Enemy enemy in enemies)
        {
            
            if (enemy != null && Vector3.Distance(tile.transform.position, enemy.transform.position) < threshold)
            {
                return true; 
            }
        }
        return false; 
    }

    // Manhattan Distance heuristic (used for A* pathfinding)
    double HeuristicsDistance(Tile currentTile, Tile goalTile)
    {
        int xdist = Math.Abs(goalTile.indexX - currentTile.indexX);
        int ydist = Math.Abs(goalTile.indexY - currentTile.indexY);
        return (xdist + ydist) * 10; 
    }

    
    Queue<Tile> RetracePath(Node node)
    {
        List<Tile> tileList = new List<Tile>();
        Node nodeIterator = node;
        while (nodeIterator.cameFrom != null)
        {
            tileList.Insert(0, nodeIterator.tile);
            nodeIterator = nodeIterator.cameFrom;
        }
        return new Queue<Tile>(tileList);
    }

    // Generate a Random Path (used for enemies)
    public Queue<Tile> RandomPath(Tile start, int stepNumber)
    {
        List<Tile> tileList = new List<Tile>();
        Tile currentTile = start;
        for (int i = 0; i < stepNumber; i++)
        {
            Tile nextTile;
            if (currentTile.Adjacents.Count < 0)
            {
                break;
            }
            else if (currentTile.Adjacents.Count == 1)
            {
                nextTile = currentTile.Adjacents[0];
            }
            else
            {
                nextTile = null;
                List<Tile> adjacentList = new List<Tile>(currentTile.Adjacents);
                ShuffleTiles<Tile>(adjacentList);
                if (tileList.Count <= 0) nextTile = adjacentList[0];
                else
                {
                    foreach (Tile tile in adjacentList)
                    {
                        if (tile != tileList[tileList.Count - 1])
                        {
                            nextTile = tile;
                            break;
                        }
                    }
                }
            }
            tileList.Add(currentTile);
            currentTile = nextTile;
        }
        return new Queue<Tile>(tileList);
    }

    private void ShuffleTiles<T>(List<T> list)
    {
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }
}

