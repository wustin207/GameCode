using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

//This script gets the heart rate value of the player and displays it as a graph in Unity.

public class GraphTest : MonoBehaviour
{
    #region Player
    private Player playerScript;
    #endregion

    #region Heart Rate
    public LineRenderer lineRenderer;
    public int maxDataPoints = 10000;
    public int maxValue = 100;
    public int updateInterval = 1;
    public int heartRate;
    public Color normalColor = Color.red;
    public Color highHeartRateColor = Color.blue;
    private float timer = 0f;
    private bool aboveThreshold = false;
    private List<GameObject> highHeartRateCircles = new List<GameObject>();
    public GameObject highHeartRateCirclePrefab;
    private Transform circleContainer;
    public GameObject textPrefab;
    private Transform textContainer;

    public int minHeartRate;
    public int maxHeartRate;
    public int averageHeartRate;
    #endregion

    private static GraphTest instance;
    private List<int> heartRates = new List<int>();

    Scene currentScene;


    void Start()
    {
        StartCoroutine(WaitForHR());

        if (instance == null)
        {
            instance = this;
            //To keep the GameManager object persist between scene changes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //To destroy duplicate GameManager objects
            Destroy(gameObject); 
        }


            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerScript = player.GetComponent<Player>();
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            //Creating a container for the circles as a child of this GameObject
            circleContainer = new GameObject("HighHeartRateCircles").transform;
            circleContainer.parent = transform;

            //Creating a container for the texts as a child of this GameObject
            textContainer = new GameObject("HighHeartRateTexts").transform;
            textContainer.parent = transform;

        InvokeRepeating("UpdateGraph", 0f, updateInterval);
        
    }

    public IEnumerator WaitForHR()
    {
        yield return new WaitForSeconds(10f);
        //Initialize minHeartRate with the first heart rate value of the player in real life
        minHeartRate = (int)hyperateSocket.heartRate;
        Debug.Log("Minimum Heart Rate: " + minHeartRate);
    }

    void UpdateGraph()
    {
        currentScene = SceneManager.GetActiveScene();

        //Get the name of the current scene
        string scene = currentScene.name;
        if (scene == "SampleScene")
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerScript = player.GetComponent<Player>();
        }

        //Output min, max, and average heart rates when a scene is unloaded
        if (scene == "Win" || scene == "Lose")
        {

            maxHeartRate = heartRates.Max();
            averageHeartRate = (int)heartRates.Average();

            GameManager manager = FindObjectOfType<GameManager>();
            manager.HeartRateAverage = averageHeartRate;
            manager.HeartRateMin = minHeartRate;
            manager.HeartRateMax = maxHeartRate;

            Debug.Log("Minimum Heart Rate: " + minHeartRate);
            Debug.Log("Maximum Heart Rate: " + maxHeartRate);
            Debug.Log("Average Heart Rate: " + averageHeartRate);
        }


        timer += updateInterval;
        AddDataPoint((int)hyperateSocket.heartRate, playerScript.CurrentHealth);

        if (lineRenderer.positionCount > maxDataPoints)
        {
            lineRenderer.positionCount--;
        }

        if (timer >= updateInterval)
        {
            //Reset the list to check for the next interval
            aboveThreshold = false; 
            timer = 0f;
        }

        //Add the current heart rate value to the list
        heartRates.Add((int)hyperateSocket.heartRate);

        if ((int)hyperateSocket.heartRate > 20 && (int)hyperateSocket.heartRate < minHeartRate)
        {
            minHeartRate = (int)hyperateSocket.heartRate;
        }

        //Ensure the heartRates list doesn't exceed the maxDataPoints
        if (heartRates.Count > maxDataPoints)
        {
            heartRates.RemoveAt(0);
        }
    }


    void AddDataPoint(int value, int health)
    {
        float normalizedX = (float)lineRenderer.positionCount / maxDataPoints;
        float xRange = 20f;
        Vector3 newPosition = new Vector3(normalizedX * xRange, (float)value / maxValue, 0f);

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPosition);

        Color pointColor = (health < 50) ? highHeartRateColor : normalColor;

        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = pointColor;
        lineRenderer.material = material;

        //Check if the current count is a multiple of 5 (example: 5, 10, 15, ...)
        if (lineRenderer.positionCount % 10 == 0)
        {

            //Create an empty GameObject as a child of this GameObject
            GameObject textContainerObject = new GameObject("HighHeartRateTextContainer");
            textContainerObject.transform.parent = textContainer;
            //Set the position to the data point position
            textContainerObject.transform.position = newPosition;

            //Create text object above the data point to display the heart rate amount of the player
            GameObject textObject = Instantiate(textPrefab, textContainerObject.transform.position, Quaternion.identity, textContainerObject.transform);
            textObject.transform.position = newPosition + Vector3.up * 0.3f;
            TextMesh textMesh = textObject.GetComponent<TextMesh>();
            //Display heart rate value
            textMesh.text = value.ToString();
        }


        if (health < 50)
        {
            if (!aboveThreshold)
            {
                //Create an empty GameObject as a child of this GameObject
                GameObject circleContainerObject = new GameObject("HighHeartRateCircleContainer");
                circleContainerObject.transform.parent = circleContainer;
                //Set the position to the data point position
                circleContainerObject.transform.position = newPosition;

                //Instantiate a circle prefab as a child of the circle container
                GameObject circle = Instantiate(highHeartRateCirclePrefab, circleContainerObject.transform.position, Quaternion.identity, circleContainerObject.transform);
                SpriteRenderer spriteRenderer = circle.GetComponent<SpriteRenderer>();
                spriteRenderer.color = Color.black;
                spriteRenderer.sortingOrder = 1;
                //Set the flag to prevent instantiating multiple circles in the same interval
                aboveThreshold = true;
            }
        }
        else
        {
            //Reset the flag if the value is over 49
            aboveThreshold = false; 
        }
    }
}
