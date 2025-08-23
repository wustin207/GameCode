using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//This script was created to control the overall state of the game, such as managing the transitions of scenes.

public class GameManager : MonoBehaviour
{
    #region UI
    [Header("UI")]
    public Timer timerDisplay;
    public Text timerText;

    public TextMeshProUGUI currentDungeonTimerText;
    public TextMeshProUGUI dungeonsClearedText;
    public TextMeshProUGUI totalTimerText;
    public TextMeshProUGUI playerKillsText;
    public TextMeshProUGUI playerDeathsText;
    #endregion

    #region Time
    [Header("Time")]
    private float startTime;
    public float currentTime;
    public static string currentDungeonTimer;
    #endregion

    #region Player & Stats
    [Header("Player & Stats")]
    public Player playerScript;
    public Stats statsScript;
    private static GameManager instance;
    #endregion

    #region HeartRate
    [Header("HeartRate")]
    public int HeartRateAverage;
    public int HeartRateMin;
    public int HeartRateMax;
    #endregion

    #region Gameplay Stats
    [Header("Gameplay Stats")]
    public static int playerKills;
    public static int playerDeaths;
    public static int RoomsCleared;
    public static int DungeonsCleared;
    public static int DungeonsFailed = 0;
    #endregion

    #region Scene
    [Header("Scene")]
    Scene currentScene;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //Make the GameManager object persist between scene changes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy duplicate GameManager objects
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "SampleScene" && startTime == 0f)
        {
            //Start timer
            startTime = Time.time;
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Check if the loaded scene is "SampleScene" and get the Timer
        if(scene.name == "SampleScene")
        {
            timerText = GameObject.FindWithTag("Timer").GetComponent<Text>();
            timerDisplay = GameObject.FindWithTag("Timer").GetComponent<Timer>();
        }

        //Check if the loaded scene is "Lose" and destroy the MainCanvas if found
        if (scene.name == "Lose")
        {
            currentDungeonTimerText = GameObject.FindWithTag("dungeonTimer").GetComponent<TextMeshProUGUI>();
            totalTimerText = GameObject.FindWithTag("totalTime").GetComponent<TextMeshProUGUI>();
            playerKillsText = GameObject.FindWithTag("playerKills").GetComponent<TextMeshProUGUI>();
            playerDeathsText = GameObject.FindWithTag("playerDeaths").GetComponent<TextMeshProUGUI>();


            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                Destroy(mainCanvas);
            }

            currentDungeonTimerText.text = currentDungeonTimer.ToString();
            playerKillsText.text = playerKills.ToString();
            playerDeathsText.text = playerDeaths.ToString();


            //Convert Time.realtimeSinceStartup to minutes, seconds, and milliseconds
            float totalTimeInSeconds = Time.realtimeSinceStartup;
            int minutes = Mathf.FloorToInt(totalTimeInSeconds / 60);
            int seconds = Mathf.FloorToInt(totalTimeInSeconds % 60);
            int milliseconds = Mathf.FloorToInt((totalTimeInSeconds * 1000) % 1000);

            totalTimerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        //Check if the loaded scene is "Win" and destroy the MainCanvas if found
        if (scene.name == "Win")
        {
            currentDungeonTimerText = GameObject.FindWithTag("dungeonTimer").GetComponent<TextMeshProUGUI>();
            totalTimerText = GameObject.FindWithTag("totalTime").GetComponent<TextMeshProUGUI>();
            playerKillsText = GameObject.FindWithTag("playerKills").GetComponent<TextMeshProUGUI>();
            playerDeathsText = GameObject.FindWithTag("playerDeaths").GetComponent<TextMeshProUGUI>();


            GameObject mainCanvas = GameObject.Find("MainCanvas");
            if (mainCanvas != null)
            {
                Destroy(mainCanvas);
            }

            currentDungeonTimerText.text = currentDungeonTimer.ToString();
            playerKillsText.text = playerKills.ToString();
            playerDeathsText.text = playerDeaths.ToString();

            //Convert Time.realtimeSinceStartup to minutes, seconds, and milliseconds
            float totalTimeInSeconds = Time.realtimeSinceStartup;
            int minutes = Mathf.FloorToInt(totalTimeInSeconds / 60);
            int seconds = Mathf.FloorToInt(totalTimeInSeconds % 60);
            int milliseconds = Mathf.FloorToInt((totalTimeInSeconds * 1000) % 1000);

            totalTimerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

            currentTime = 0f;
            startTime = Time.time;
        }
    }

    private void OnEnable()
    {
        //Subscribe to the event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //Unsubscribe from the event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Update()
    {

        currentScene = SceneManager.GetActiveScene();

        //Get the name of the current scene
        string scene = currentScene.name;

        if (scene == "SampleScene")
        {
            UpdateStartTimer();
        }

        else if(scene == "Lose")
        {
            //if the player presses "R", the game restarts
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("SampleScene");
            }
            //else if the player presses "Q" the game gets and sends the player data and then shuts down
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                statsScript = GameObject.Find("Stats").GetComponent<Stats>();
                statsScript.GetStats();
                StartCoroutine(WaitforSave());
            }
        }

        else if(scene == "Win")
        {
            //if the player presses "C", the game continues
            if (Input.GetKeyDown(KeyCode.C))
            {
                SceneManager.LoadScene("SampleScene");
            }
            //else if the player presses "Q" the game gets and sends the player data and then shuts down
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                statsScript = GameObject.Find("Stats").GetComponent<Stats>();
                statsScript.GetStats();
                StartCoroutine(WaitforSave());
            }
        }

        //When the player dies save the total session time
        if (Player.canTakeDamage == false)
        {
            currentDungeonTimer = timerText.text;
        }

    }

    IEnumerator WaitforSave()
    {
        Debug.Log("Quitting game...");
        yield return new WaitForSeconds(2);
        Application.Quit();
    }

    //Structure of the timer
    private void UpdateStartTimer()
    {
        currentTime = Time.time - startTime;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

        string timeString = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        timerText.text = timeString;
    }


}
