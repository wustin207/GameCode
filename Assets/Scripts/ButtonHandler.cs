using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Website()
    {
        Application.OpenURL("https://server-6stn.onrender.com/api/stats");
    }
}
