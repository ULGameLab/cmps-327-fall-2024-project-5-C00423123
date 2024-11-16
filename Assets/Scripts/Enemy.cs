using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior { EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    // Pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    // Properties
    public float speed = 1.0f;
    public float visionDistance = 5.0f; // Player detection range
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1;

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        // Initialize PathFinder with the list of enemies in the scene
        pathFinder = new PathFinder(new List<Enemy>(GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None)));
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            return;
        }

        switch (behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }
    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Handle EnemyBehavior1 (Random Walking)
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT:
                material.color = Color.white;

                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // Handle EnemyBehavior2 (Chasing the Player)
    private void HandleEnemyBehavior2()
    {
        switch (state)
        {
            case EnemyState.DEFAULT:
                material.color = Color.red; 

                

                // Check if the player is within vision distance
                if (Vector3.Distance(transform.position, playerGameObject.transform.position) < visionDistance)
                {
                    Debug.Log("player detected. going to chase?");
                    state = EnemyState.CHASE;
                }
                else
                {
                    // Use the PathFinder to find a path to the player's last known position
                    if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);
                    if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
            
                break;
            case EnemyState.CHASE:
                material.color=Color.blue;
                Debug.Log("chase started");

                Tile playerCurrentTile = playerGameObject.GetComponent<Player>().currentTile;
                path = pathFinder.FindPathAStar(currentTile, playerCurrentTile);
                
                if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                break;


            case EnemyState.MOVING:
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                // If target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                    Debug.Log("reached");
                }

                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    //  EnemyBehavior3  Select a tile a few tiles away from the player, then chase
    private void HandleEnemyBehavior3()
{
    switch (state)
    {
        case EnemyState.DEFAULT:
            material.color = Color.yellow;  
            // If the path is empty, calculate a random path
            if (path.Count <= 0)
            {
                path = pathFinder.RandomPath(currentTile, 20);  
            }

            if (path.Count > 0)
            {
                targetTile = path.Dequeue();
                state = EnemyState.MOVING;  
            }

            // Check if the player is within vision distance
            if (Vector3.Distance(transform.position, playerGameObject.transform.position) < visionDistance)
            {

                // Player detected, calculate a target tile a few tiles away from the player
                Vector3 playerPosition = playerGameObject.transform.position;
                Vector3 directionAwayFromPlayer = (transform.position - playerPosition).normalized;

                // Calculate the target position a few tiles away from the player (away from the player)
                Vector3 newTargetPosition = playerPosition + directionAwayFromPlayer * 2.0f;  
                Tile newTargetTile = GetTileFromPosition(newTargetPosition);

                // If we have a valid new target tile, calculate a path to it
                if (newTargetTile != null && path.Count <= 0)
                {
                    path = pathFinder.FindPathAStar(currentTile, newTargetTile);  
                }

                // If a valid path to the new target tile is found, move to that tile
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;  

                }
            }
            break;

        case EnemyState.CHASE:
            material.color = Color.black; //changed coloring to see when it was reacting
            // Find the tile the player is currently on
            Tile playerCurrentTile = playerGameObject.GetComponent<Player>().currentTile;

            // Use pathfinding to find a path to the player's current tile
            path = pathFinder.FindPathAStar(currentTile, playerCurrentTile);

            // If a valid path is found, start moving towards the player
            if (path.Count > 0)
            {
                targetTile = path.Dequeue();
                state = EnemyState.MOVING; 
            }
            break;

        case EnemyState.MOVING:
            material.color = Color.yellow;  

            // Calculate the direction to move towards the target tile
            velocity = targetTile.gameObject.transform.position - transform.position;
            transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

            // If target reached, check the next state
            if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
            {
                currentTile = targetTile;  
                // After reaching the target tile, switch to chase mode
                if (state == EnemyState.MOVING)
                {
        
                    state = EnemyState.CHASE;  // Transition to CHASE state
                }
            }
            break;

        default:
            state = EnemyState.DEFAULT;  // Default state when no other condition matches
            break;
    }
}
    private Tile GetTileFromPosition(Vector3 position)//added this function because enemy3 was not wanting to chase
{
    Tile closestTile = null;
    float closestDistance = Mathf.Infinity;

    foreach (Tile tile in GameObject.FindObjectsByType<Tile>(FindObjectsSortMode.None))
    {
        float distance = Vector3.Distance(position, tile.transform.position);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestTile = tile;
        }
    }

 
    if (closestTile == null)
    {
        
    }

    return closestTile;
}

}


