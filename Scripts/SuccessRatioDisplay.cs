using UnityEngine;
using UnityEngine.UI;

//This script displays the player's total wins and deaths in the in-game UI as text.

public class SuccessRatioDisplay : MonoBehaviour
{
    public Text ratioText;
    public int totalAttempts => GameManager.DungeonsCleared + GameManager.DungeonsFailed;

    private void Awake()
    {
        UpdateSuccessRatioText();
        DontDestroyOnLoad(transform.root.gameObject);
    }

    

    public void UpdateSuccessRatioText()
    {
        //Ensures that DungeonsCleared and DungeonsFailed are not zero to prevent division by zero
        if (GameManager.DungeonsCleared == 0 && GameManager.DungeonsFailed == 0)
        {
            ratioText.text = "Success / Fail Rate: 0.0%";
            return;
        }

        //Calculate the success and death ratios of the player
        float successRatio = (float)GameManager.DungeonsCleared / (GameManager.DungeonsCleared + GameManager.DungeonsFailed);
        float deathRatio = (float)GameManager.DungeonsFailed / (GameManager.DungeonsCleared + GameManager.DungeonsFailed);

        //Format the ratios to display as percentages in the text
        string ratioTextValue = string.Format(
            "Success / Fail Rate: {0}%",
            (successRatio * 100).ToString("F1"),
            (deathRatio * 100).ToString("F1")
        );

        //Updates the Text component with the new value
        ratioText.text = ratioTextValue;
    }
}
