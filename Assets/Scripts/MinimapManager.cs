using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    #region UI
    [Header("UI")]
    public RectTransform iconsContainer;
    public GameObject playerDotPrefab;
    public GameObject enemyDotPrefab;
    public GameObject itemDotPrefab;
    #endregion

    #region Player
    [Header("Player")]
    public Transform playerTransform;
    #endregion

    #region Minimap Settings
    [Header("Minimap Settings")]
    public float mapScale = 4f;
    public Vector3 mapOrigin = Vector3.zero;
    #endregion

    #region Icons
    [Header("Icons")]
    private MinimapIcon playerIcon;
    private List<MinimapIcon> enemyIcons = new List<MinimapIcon>();
    private List<MinimapIcon> itemIcons = new List<MinimapIcon>();
    #endregion

    void Start()
    {
        SpawnPlayerDot();
        SpawnEnemyDots();
        SpawnItemDots();
    }

    public void SpawnPlayerDot()
    {
        //If icons already exist, destroy this one
        if (playerIcon != null && playerIcon.icon != null)
        {
            Destroy(playerIcon.icon.gameObject);
        }

        GameObject iconsContainerObject = GameObject.FindGameObjectWithTag("icon");
        iconsContainer = iconsContainerObject.GetComponent<RectTransform>();

        //Spawn the icon of the player in the minimap
        GameObject playerDot = Instantiate(playerDotPrefab, iconsContainer);
        playerIcon = playerDot.AddComponent<MinimapIcon>();
        playerIcon.icon = playerDot.GetComponent<RectTransform>();

        playerIcon.target = playerTransform;
        playerIcon.mapScale = mapScale;
        playerIcon.playerTransform = playerTransform;
    }


    public void SpawnEnemyDots()
    {
        //Put all enemies in a list
        List<GameObject> allEnemies = new List<GameObject>();
        string[] enemyTags = { "Zombie", "Skeleton", "Archer" };

        //Add tagged enemies in 1 single list (allEnemies)
        foreach (string tag in enemyTags)
        {
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag(tag);
            allEnemies.AddRange(taggedEnemies);
        }
        //For every enemy, add a dot in the minimap
        foreach (GameObject enemy in allEnemies)
        {
            GameObject iconsContainerObject = GameObject.FindGameObjectWithTag("icon");
            iconsContainer = iconsContainerObject.GetComponent<RectTransform>();
            GameObject enemyDot = Instantiate(enemyDotPrefab, iconsContainer);
            MinimapIcon iconScript = enemyDot.AddComponent<MinimapIcon>();
            iconScript.target = enemy.transform;
            iconScript.icon = enemyDot.GetComponent<RectTransform>();
            iconScript.mapScale = mapScale;
            iconScript.playerTransform = playerTransform;

            enemyIcons.Add(iconScript);
        }
    }

    //For every health item, add a dot in the minimap
    public void SpawnItemDots()
    {
        var items = GameObject.FindGameObjectsWithTag("Health");
        foreach (var item in items)
        {
            GameObject iconsContainerObject = GameObject.FindGameObjectWithTag("icon");
            iconsContainer = iconsContainerObject.GetComponent<RectTransform>();
            GameObject itemDot = Instantiate(itemDotPrefab, iconsContainer);
            MinimapIcon iconScript = itemDot.AddComponent<MinimapIcon>();
            iconScript.target = item.transform;
            iconScript.icon = itemDot.GetComponent<RectTransform>();
            iconScript.mapScale = mapScale;
            iconScript.playerTransform = playerTransform;

            itemIcons.Add(iconScript);
        }
    }


}
