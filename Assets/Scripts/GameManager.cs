using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//This script was created to control the overall state of the game, such as managing the transitions of scenes.

public class GameManager : MonoBehaviour
{
    public Timer timerDisplay;
    public Text timerText;
    private float startTime;
    public float currentTime;
    public Player playerScript;

    public static int playerKills;
    public static int playerDeaths;
    public static int RoomsCleared;
    public static int DungeonsCleared;
    public static int DungeonsFailed = 0;
    public static string currentDungeonTimer;
    public TextMeshProUGUI currentDungeonTimerText;
    public TextMeshProUGUI dungeonsClearedText;
    public TextMeshProUGUI totalTimerText;
    public TextMeshProUGUI playerKillsText;
    public TextMeshProUGUI playerDeathsText;
    Scene currentScene;

    private static GameManager instance;

    private void Start()
    {

        currentScene = SceneManager.GetActiveScene();

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

        if (currentScene.name == "SampleScene" && startTime == 0f)
        {
            startTime = Time.time;
        }

    }


    // Called when a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

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

        if (scene.name == "Win")
        {
            currentDungeonTimerText = GameObject.FindWithTag("dungeonTimer").GetComponent<TextMeshProUGUI>();
            //dungeonsClearedText = GameObject.FindWithTag("dungeonCleared").GetComponent<TextMeshProUGUI>();
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
            //dungeonsClearedText.text = DungeonsCleared.ToString();


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
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("SampleScene");
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Quitting game...");
                Application.Quit();
            }
        }

        else if(scene == "Win")
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Loading SampleScene...");
                SceneManager.LoadScene("SampleScene");
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Quitting game...");
                Application.Quit();
            }
        }

        //When the player dies save the total session time
        if (Player.canTakeDamage == false)
        {
            currentDungeonTimer = timerText.text;
        }

    }



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
