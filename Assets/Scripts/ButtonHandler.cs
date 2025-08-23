using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    //Loads the game scene
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    //Loads the website link
    public void Website()
    {
        Application.OpenURL("https://website-9lka.onrender.com/");
    }
}
