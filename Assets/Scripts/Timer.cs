using UnityEngine;
using UnityEngine.UI;

//This script handles the timer of the game which increases every second.

public class Timer : MonoBehaviour
{
    public Text timerText;
    private float startTime;
    private bool isGameRunning = true;

    private void Start()
    {
        if (timerText == null)
        {
            //Disabling the script to avoid further errors.
            enabled = false;
            return;
        }

    }

    //Start the timer
    public void StartTimer()
    {
        startTime = Time.time;
        UpdateTimer();
    }

    private void Update()
    {
        if (isGameRunning)
        {
            UpdateTimer();
        }
    }

    //Update the timer (Every frame) and show in minutes, second, milliseconds
    private void UpdateTimer()
    {
        float currentTime = Time.time - startTime;
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        int milliseconds = Mathf.FloorToInt((currentTime * 100f) % 100f);

        string timeString = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        timerText.text = timeString;
    }

    //When the game stops, stop updating the timer.
    public void StopTimer()
    {
        isGameRunning = false;
    }
}
