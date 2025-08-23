using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

//This script procedurely generates the content of the level, so that every level feels different.

public class PCG : MonoBehaviour
{
    #region Enemies
    [Header("Enemies")]
    public GameObject[] enemyPrefabs;
    public int minEnemyCount = 4;
    public int maxEnemyCount = 7;
    public int minTrapCount;
    public int maxTrapCount;
    public int minDistanceToEnemies = 10;
    #endregion

    #region Items
    [Header("Items")]
    public GameObject itemPrefab;
    public int minItemCount;
    public int maxItemCount;
    #endregion

    #region Environment Prefabs
    [Header("Environment Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject roofPrefab;
    public GameObject trapDoorPrefab;
    public GameObject trapSpikesPrefab;
    public GameObject portalPrefab;
    public GameObject playerPrefab;
    #endregion

    #region Map Generation
    [Header("Map")]
    public int minWidth;
    public int maxWidth;
    public int minHeight;
    public int maxHeight;
    public int fillPercentage;
    public int smoothingIterations;
    public float lightIntensity;
    public int[,] map;
    public int width;
    public int height;
    #endregion

    #region Components
    [Header("Components")]
    public SuccessRatioDisplay successRatio;
    private CharacterController characterController;
    private NavMeshSurface navMeshSurface;
    private DDA ddaScript;
    #endregion

    #region UI
    [Header("UI")]
    public Image fadePanel;
    private float fadeDuration = 5f;
    #endregion

    private void Start()
    {
        //The minimum and maximum enemies can spawn
        minEnemyCount = 4;
        maxEnemyCount = 7;
        //The minimumm and maximum traps can spawn
        minTrapCount = 1;
        maxTrapCount = 6;
        //The minimumm and maximum of health items that can spawn
        minItemCount = 1;
        maxItemCount = 4;
        

        characterController = playerPrefab.GetComponent<CharacterController>();
        //The minimum and maximum width the Dungeon can get randomized
        minWidth = 50;
        maxWidth = 100;
        //Randomize the width and height of the Dungeon
        width = Random.Range(minWidth, maxWidth + 1);
        height = Random.Range(minHeight, maxHeight + 1);

        GameManager.RoomsCleared = 0;
        navMeshSurface = GetComponent<NavMeshSurface>();
        lightIntensity = 0.5f;
        fillPercentage = 38;
        smoothingIterations = 100;

        //Find the DDA script in the scene
        ddaScript = FindObjectOfType<DDA>();

        GenerateMap();
    }

    public void GenerateMap()
    {
        //If the player completes more than 3 levels, the game loads the win scene.
        if (GameManager.RoomsCleared > 3)
        {
            Debug.Log("Game Finished");
            WinScene();
        }
        //Else a new map generates randomly
        else
        {
            map = new int[width, height];
            RandomFillMap();

            for (int i = 0; i < smoothingIterations; i++)
            {
                CellularAutomata();
            }
            ddaScript.UpdateDDA();
            DrawMap();
            SpawnItems();
            SpawnTrapDoors();
            SpawnTrapSpikes();
            //WaitForNav is done to make sure that the navmesh has correctly baking everything after everything is loaded
            StartCoroutine(WaitForNav());
        }
    }

    public IEnumerator WaitForNav()
    {
        yield return new WaitForSeconds(4f);
        navMeshSurface.BuildNavMesh();
        SpawnEnemies();
        SpawnPortal();
        FindObjectOfType<MinimapManager>().SpawnPlayerDot();
        FindObjectOfType<MinimapManager>().SpawnEnemyDots();
        FindObjectOfType<MinimapManager>().SpawnItemDots();
        //Check if the player can reach all enemies and the portal
        if (CanReachAllEnemies() && CanReachPortal())
        {
            Debug.Log("Reachable");
        }
        //If not, reload the game to prevent from the player getting stuck
        else
        {
            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                Destroy(mainCanvas);
            }
            SceneManager.LoadScene("SampleScene");
        }
    }

    //This method ensures that the player can reach the enemies after the level is randomised, this is done by calculating the path of the player to the enemies
    private bool CanReachAllEnemies()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return false;
        }

        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemies = GameObject.FindGameObjectsWithTag(enemyPrefab.tag);
            foreach (var enemy in enemies)
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(player.transform.position, enemy.transform.position, NavMesh.AllAreas, path))
                {
                    if (path.status != NavMeshPathStatus.PathComplete)
                    {
                        Debug.LogWarning("Player cannot reach an enemy.");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning("Failed to calculate path to an enemy.");
                    return false;
                }
            }
        }
        return true;
    }

    //This method ensures that the player can reach the portal after the level is randomised, this is done by calculating the path of the player to the portal
    private bool CanReachPortal()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return false;
        }

        GameObject portal = GameObject.FindGameObjectWithTag("Portal");
        if (portal == null)
        {
            Debug.LogError("Portal not found.");
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(player.transform.position, portal.transform.position, NavMesh.AllAreas, path))
        {
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.LogWarning("Player cannot reach the portal.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Failed to calculate path to the portal.");
            return false;
        }

        return true;
    }

    #region Cellular Automata

    //The following 2 methods uses Celullar Automata algorithms to randomly generate a cave like structure level.
    private void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    //Place walls along the edges
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (Random.Range(0, 100) < fillPercentage) ? 1 : 0;
                }
            }
        }
    }

    private void CellularAutomata()
    {
        int[,] newMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int wallCount = GetSurroundingWallCount(x, y);

                if (wallCount > 4)
                {
                    newMap[x, y] = 1;
                }
                else if (wallCount < 4)
                {
                    newMap[x, y] = 0;
                }
                else
                {
                    newMap[x, y] = map[x, y];
                }
            }
        }

        map = newMap;
    }
    #endregion

    private void DrawMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                Vector3 roofPosition = new Vector3(x, 10.5f, y);
                //Spawn the walls and floor of the dungeon
                GameObject prefab = (map[x, y] == 1) ? wallPrefab : floorPrefab;
                Instantiate(prefab, position, Quaternion.identity, transform);
                //Spawn the roof of the dungeon
                Instantiate(roofPrefab, roofPosition, Quaternion.identity, transform);
            }
        }

        int roomCenterX = width / 2;
        int roomCenterY = height / 2;

        //Calculate the center position of the room
        Vector3 roomCenterPosition = new Vector3(roomCenterX, 8.5f, roomCenterY);

        //Calculate the corner positions of the room
        Vector3 topLeftCorner = new Vector3(1, 8.5f, 1);
        Vector3 topRightCorner = new Vector3(width - 1, 8.5f, 1);
        Vector3 bottomLeftCorner = new Vector3(1, 8.5f, height - 1);
        Vector3 bottomRightCorner = new Vector3(width - 1, 8.5f, height - 1);

        //Find all game objects with the Light tag
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("Light");

        //Iterate through the array and destroy each Light object
        foreach (GameObject obj in objectsWithTag)
        {
            Destroy(obj);
        }

        //Spawn lights at the center and corners
        SpawnLightAtPosition(roomCenterPosition);
        SpawnLightAtPosition(topLeftCorner);
        SpawnLightAtPosition(topRightCorner);
        SpawnLightAtPosition(bottomLeftCorner);
        SpawnLightAtPosition(bottomRightCorner);



        //Spawn the player
        Vector3 playerSpawnPosition = FindPlayerSpawnPosition();
        //If its the first match of the player, instantiate a player prefab
        if (GameManager.RoomsCleared < 1)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
            player.name = "Player(Clone)";

        }
        //If not, reposition the existing player
        else
        {
            //Destroy the previous player GameObject
            GameObject existingPlayer = GameObject.Find("Player(Clone)");
            //Reposition the existing player object
            existingPlayer.transform.position = playerSpawnPosition;
        }

        MinimapManager minimapManager = FindObjectOfType<MinimapManager>();
        minimapManager.playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        //Spawn all dots icons on the minimap
        minimapManager.SpawnPlayerDot();
        minimapManager.SpawnEnemyDots();
        minimapManager.SpawnItemDots();
    }


    #region Enemies
    private void SpawnEnemies()
    {
        int zombieCount = Random.Range(minEnemyCount, maxEnemyCount);

        for (int i = 0; i < zombieCount; i++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);
            //Ensure zombies don't spawn inside walls
            while (map[randomX, randomY] == 1)
            {
                randomX = Random.Range(0, width);
                randomY = Random.Range(0, height);
            }

            Vector3 position = new Vector3(randomX, 1, randomY);

            //Choose a random enemy prefab from the array
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            //Instantiate the chosen enemy prefab
            Instantiate(randomEnemyPrefab, position, Quaternion.identity, transform);
        }
    }

    //Method that checks if there are any enemies remaining in the level.
    public bool AreEnemiesRemaining()
    {
        //Count all enemies in the scene based on their tags
        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemies = GameObject.FindGameObjectsWithTag(enemyPrefab.tag);
            if (enemies.Length > 0)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Player
    private Vector3 FindPlayerSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;

        //Find a random floor tile that is not too close to the enemies
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            if (map[randomX, randomY] == 0 && !IsPositionNearEnemies(randomX, randomY))
            {
                spawnPosition = new Vector3(randomX, 1.2f, randomY);
                break;
            }

            attempts++;
        }

        return spawnPosition;
    }

    private bool IsPositionNearEnemies(int x, int y)
    {
        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemies = GameObject.FindGameObjectsWithTag(enemyPrefab.tag);
            foreach (var enemy in enemies)
            {
                Vector3 enemyPosition = enemy.transform.position;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(enemyPosition.x, enemyPosition.z));
                if (distance < minDistanceToEnemies)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Items
    private void SpawnItems()
    {
        //Randomise the item count within the minitemCount and maxItemCount range
        int itemCount = Random.Range(minItemCount, maxItemCount + 1);

        for (int i = 0; i < itemCount; i++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            //Ensure items don't spawn in walls
            while (map[randomX, randomY] == 1)
            {
                randomX = Random.Range(0, width);
                randomY = Random.Range(0, height);
            }

            Vector3 position = new Vector3(randomX, 1.2f, randomY);
            Instantiate(itemPrefab, position, Quaternion.identity, transform);
        }
    }
    #endregion

    private void SpawnTrapDoors()
    {
        int trapDoorCount = Random.Range(minTrapCount, maxTrapCount);

        for (int i = 0; i < trapDoorCount; i++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            //Ensure trap door doesn't spawn in walls and other objects
            while (map[randomX, randomY] == 1 || IsTrapPositionInvalid(randomX, randomY))
            {
                randomX = Random.Range(0, width);
                randomY = Random.Range(0, height);
            }

            //Store the position of the floor tile before destroying it
            Vector3 floorPosition = new Vector3(randomX, 0, randomY);

            //Raycast downward to find the floor tile
            RaycastHit hit;
            if (Physics.Raycast(floorPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                if (hit.collider.gameObject.name == "Floor" || hit.collider.gameObject.name == "Floor(Clone)")
                {
                    //Destroy the floor cell at the position of the trap door
                    Destroy(hit.collider.gameObject);

                    //Update the level to indicate the floor cell has been removed
                    map[randomX, randomY] = 1;

                    Vector3 position = new Vector3(randomX, 0.45f, randomY);

                    //Instantiate the trap door prefab
                    Instantiate(trapDoorPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    Debug.LogWarning("No floor cell found under the trap door position.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast didn't hit anything under the trap door position.");
            }
        }
    }


    private void SpawnTrapSpikes()
    {
        int trapDoorCount = Random.Range(minTrapCount, maxTrapCount);

        for (int i = 0; i < trapDoorCount; i++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            //Ensure the trap spikes don't spawn inside walls and other objects
            while (map[randomX, randomY] == 1 || IsTrapPositionInvalid(randomX, randomY))
            {
                randomX = Random.Range(0, width);
                randomY = Random.Range(0, height);
            }

            Vector3 position = new Vector3(randomX, 0.55f, randomY);
            Instantiate(trapSpikesPrefab, position, Quaternion.identity, transform);
        }
    }

    private bool IsTrapPositionInvalid(int x, int y)
    {
        //Check if the position overlaps with items tagged as "Spike" or "Trapdoor" to ensure that traps don't spawn inside each other.
        Collider[] colliders = Physics.OverlapSphere(new Vector3(x, 0, y), 0.5f);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Spike") || collider.CompareTag("Trapdoor") || collider.CompareTag("Player") || collider.CompareTag("Portal"))
            {
                Debug.Log("Object hit another object, respawning");
                return true;
            }
        }

        return false;
    }



    #region Portal
    private void SpawnPortal()
    {
        int randomX = Random.Range(0, width);
        int randomY = Random.Range(0, height);

        //Keep re-randomising until the position is not close to a wall
        while (map[randomX, randomY] == 1 || IsPortalCloseToWall(randomX, randomY, 2))
        {
            Debug.Log("Spawned in a wall");
            randomX = Random.Range(0, width);
            randomY = Random.Range(0, height);
        }

        //Set the position
        Vector3 position = new Vector3(randomX, 0.55f, randomY);
        //Set the rotation of the portal to -90f in the X axis
        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
        Instantiate(portalPrefab, position, rotation, transform);
    }

    //Checks if the portal spawns too close of a wall
    private bool IsPortalCloseToWall(int x, int y, int minDistance)
    {
        for (int i = Mathf.Max(0, x - minDistance); i < Mathf.Min(width, x + minDistance + 1); i++)
        {
            for (int j = Mathf.Max(0, y - minDistance); j < Mathf.Min(height, y + minDistance + 1); j++)
            {
                //If portal is close return true
                if (map[i, j] == 1)
                {
                    return true;
                }
            }
        }
        //If portal is not close return false
        return false;
    }
    #endregion

    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int wallCount = GetSurroundingWallCount(x, y);
                if (wallCount > 4)
                {
                    map[x, y] = 1;
                }
                else if (wallCount < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    //Method to check how many walls are near the object
    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    private void SpawnLightAtPosition(Vector3 position)
    {
        //Create an empty gameObject
        GameObject lightGameObject = new GameObject("DungeonLight");
        //Set the position of the light
        lightGameObject.transform.position = position;
        //Add a component called "Light"
        Light lightComponent = lightGameObject.AddComponent<Light>();
        lightComponent.range = 200f;
        //Set the light intensity
        lightComponent.intensity = lightIntensity;
        //Give the gameObject a tag "Light"
        lightComponent.tag = "Light";
    }

    //Method to destroy the dungeon room
    public void DestroyRoom()
    {
        NavMesh.RemoveAllNavMeshData();
        //Destroy all children objects of the DungeonGenerator (walls, floor, enemies, items, and the portal)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    //Method to switch to the Win scene
    public void WinScene()
    {
        //Fade to black to smoothly transition the scene
        StartCoroutine(FadeToBlack());
        GameManager.DungeonsCleared++;
        successRatio.UpdateSuccessRatioText();
        StartCoroutine(LoadWin());
    }

    public IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        Color c = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 1f;
        fadePanel.color = c;
    }

    IEnumerator LoadWin()
    {
        //Turn off canTakeDamage so that the player can't get hurt when the win loading is in progress
        Player.canTakeDamage = false;
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Win");
    }
}
