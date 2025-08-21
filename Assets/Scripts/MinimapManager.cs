using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public RectTransform iconsContainer;
    public GameObject playerDotPrefab;
    public GameObject enemyDotPrefab;
    public GameObject itemDotPrefab;

    public Transform playerTransform;

    public float mapScale = 4f;
    public Vector3 mapOrigin = Vector3.zero;

    private MinimapIcon playerIcon;
    private List<MinimapIcon> enemyIcons = new List<MinimapIcon>();
    private List<MinimapIcon> itemIcons = new List<MinimapIcon>();

    void Start()
    {
        SpawnPlayerDot();
        SpawnEnemyDots();
        SpawnItemDots();
    }

    public void SpawnPlayerDot()
    {

        if (playerIcon != null && playerIcon.icon != null)
        {
            Destroy(playerIcon.icon.gameObject);
        }

        GameObject iconsContainerObject = GameObject.FindGameObjectWithTag("icon");
        iconsContainer = iconsContainerObject.GetComponent<RectTransform>();

        GameObject playerDot = Instantiate(playerDotPrefab, iconsContainer);
        playerIcon = playerDot.AddComponent<MinimapIcon>();
        playerIcon.icon = playerDot.GetComponent<RectTransform>();

        playerIcon.target = playerTransform;
        playerIcon.mapScale = mapScale;
        playerIcon.playerTransform = playerTransform;
    }


    public void SpawnEnemyDots()
    {
        List<GameObject> allEnemies = new List<GameObject>();
        string[] enemyTags = { "Zombie", "Skeleton", "Archer" };

        foreach (string tag in enemyTags)
        {
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag(tag);
            allEnemies.AddRange(taggedEnemies);
        }

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

            Debug.Log("Created icon for enemy: " + enemy.name);

        }
    }

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
