using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


//This script Dynamically adjusts the game difficulty by using Dynamic Difficulty Adjustment algorithms (DDA).

public class DDA : MonoBehaviour
{
    private Player player;
    private PCG pcgScript;
    private Zombie[] zombies;
    private SkeletonAI[] skeletons;

    public float minDifficultyChangeRate;
    public float maxDifficultyChangeRate;

    public float increaseDifficultyThreshold;
    public float decreaseDifficultyThreshold;

    public bool ScriptLoaded = false;

    private void Start()
    {
        //Add/Remove between 1 to 5 enemies, depending on the health of the player
        minDifficultyChangeRate = 1f;
        maxDifficultyChangeRate = 5f;
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            Debug.Log("SampleScene found");
            StartCoroutine(DelayedInitialization());
        }
    }

    //When the game fully loads, load the player, and pcg scripts.
    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(2f);

        player = FindObjectOfType<Player>();
        pcgScript = FindObjectOfType<PCG>();

        if (player == null)
        {
            Debug.LogError("Player script not found in the scene.");
        }
        if (pcgScript == null)
        {
            Debug.LogError("PCG script not found in the scene.");
        }
        ScriptLoaded = true;
    }

    public void UpdateDDA()
    {
        if (ScriptLoaded)
        {
            float healthFactor = (float)player.currentHealth / Mathf.Max(1, player.maxHealth);
            float difficultyChangeRate = CalculateDifficultyChangeRate(healthFactor);
            var RandomEffect = Random.Range(1,3);

            //Inrease the difficulty if the health of the player is more than half of the total health ( - 1)
            //Decrease the difficulty if the health of the player is less than half of the total health
            increaseDifficultyThreshold = player.maxHealth / 2 - 1;
            decreaseDifficultyThreshold = player.maxHealth / 2;

            //There are 2 random effects that can be triggered when the difficulty increases/decreases: 
            //Effect 1: Modifies enemy count and health.
            //Effect 2: Adjusts lighting visibility and increases or decreases the number of traps in the level.
            if (player.currentHealth > increaseDifficultyThreshold)
            {
                if(RandomEffect == 1)
                {
                    IncreaseDifficulty(difficultyChangeRate);
                    StartCoroutine(IncreaseEnemyHealth());
                    Debug.Log("Random Effect is: " + RandomEffect);
                    Debug.Log("Current health is: " + player.currentHealth);
                }
                else if (RandomEffect == 2)
                {
                    IncreaseLightsDifficulty(difficultyChangeRate);
                    IncreaseTraps(difficultyChangeRate);
                    Debug.Log("Random Effect is: " + RandomEffect);
                    Debug.Log("Current health is: " + player.currentHealth);
                }
            }
            else if (player.currentHealth < decreaseDifficultyThreshold)
            {
                if(RandomEffect == 1)
                {
                    DecreaseDifficulty(difficultyChangeRate);
                    StartCoroutine(DecreaseEnemyHealth());
                    Debug.Log("Random Effect is: " + RandomEffect);
                    Debug.Log("Current health is: " + player.currentHealth);
                }
                else if (RandomEffect == 2)
                {
                    DecreaseLightsDifficulty(difficultyChangeRate);
                    DecreaseTraps(difficultyChangeRate);
                    Debug.Log("Random Effect is: " + RandomEffect);
                    Debug.Log("Current health is: " + player.currentHealth);

                }
            }
        }
    }

    #region EnemiesDifficulty

    //This method calculates the rate of difficulty based on the player's health.
    //The higher the player health is to max health, the higher the difficulty of the game increases.
    private float CalculateDifficultyChangeRate(float ratio)
    {
        float adjustedRatio = Mathf.Abs(ratio - 0.5f) * 2f;
        return Mathf.Lerp(minDifficultyChangeRate, maxDifficultyChangeRate, adjustedRatio);
    }

    //Increase the game difficulty by increasing the amount of enemies
    private void IncreaseDifficulty(float rate)
    {
        pcgScript.minZombieCount += Mathf.RoundToInt(rate + GameManager.DungeonsCleared);
        pcgScript.maxZombieCount += Mathf.RoundToInt(rate + GameManager.DungeonsCleared);

        Debug.Log("Increasing game difficulty with rate of " + rate);
        Debug.Log("MinZombieCount: " + pcgScript.minZombieCount);
        Debug.Log("MaxZombieCount: " + pcgScript.maxZombieCount);
    }

    //Decrease the game difficulty by decreasing the amount of enemies
    private void DecreaseDifficulty(float rate)
    {
        //Mathf.Max ensures that minZombieCount does not go below 4
        pcgScript.minZombieCount = Mathf.Max(4, pcgScript.minZombieCount - Mathf.RoundToInt(rate));
        //Mathf.Max ensure that maxZombieCount is greater than or equal to minZombieCount
        pcgScript.maxZombieCount = Mathf.Max(pcgScript.minZombieCount, pcgScript.maxZombieCount - Mathf.RoundToInt(rate));


        Debug.Log("Decreasing game difficulty with rate of " + rate);
        Debug.Log("MinZombieCount: " + pcgScript.minZombieCount);
        Debug.Log("MaxZombieCount: " + pcgScript.maxZombieCount);

    }

    //Increase the game difficulty by increasing the health of the enemies.
    private IEnumerator IncreaseEnemyHealth()
    {
        yield return new WaitForSeconds(5f);
        zombies = FindObjectsOfType<Zombie>();
        skeletons = FindObjectsOfType<SkeletonAI>();

        if (zombies == null)
        {
            Debug.LogError("Zombie script not found in the scene.");
        }
        if (skeletons == null)
        {
            Debug.LogError("Skeleton script not found in the scene.");
        }

        foreach (var zombie in zombies)
        {
            zombie.maxHealth = 15 + GameManager.DungeonsCleared;
        }

        foreach (var skeleton in skeletons)
        {
            skeleton.maxHealth = 15 + GameManager.DungeonsCleared;
        }
    }

    //Decrease the game difficulty by decreasing the health of the enemies.
    private IEnumerator DecreaseEnemyHealth()
    {
        yield return new WaitForSeconds(5f);
        zombies = FindObjectsOfType<Zombie>();
        skeletons = FindObjectsOfType<SkeletonAI>();

        if (zombies == null)
        {
            Debug.LogError("Zombie script not found in the scene.");
        }
        if (skeletons == null)
        {
            Debug.LogError("Skeleton script not found in the scene.");
        }

        foreach (var zombie in zombies)
        {
            zombie.maxHealth = 8;
        }

        foreach (var skeleton in skeletons)
        {
             skeleton.maxHealth = 8;
        }
    }

    #endregion

    #region LightsDifficulty
    //Increase the game difficulty by lowering the amount of light available in the level.
    private void IncreaseLightsDifficulty(float rate)
    {
        pcgScript.lightIntensity -= (rate + GameManager.DungeonsCleared * 0.1f) / 10f;

        //Ensure that light intensity does not go below a minimum threshold (0.05f)
        pcgScript.lightIntensity = Mathf.Max(0.05f, pcgScript.lightIntensity);

        Debug.Log("Decreased light intensity. Current intensity: " + pcgScript.lightIntensity);
    }

    //Decrease the game difficulty by increasing the amount of light available in the level.
    private void DecreaseLightsDifficulty(float rate)
    {
        pcgScript.lightIntensity += rate / 10f;

        //Ensure that light intensity does not exceed a maximum threshold (1f)
        pcgScript.lightIntensity = Mathf.Min(1f, pcgScript.lightIntensity);

        Debug.Log("Increased light intensity. Current intensity: " + pcgScript.lightIntensity);
    }

    #endregion

    #region TrapsDifficulty

    //Increase the game difficulty by increasing the amount of traps available in the level.
    private void IncreaseTraps(float rate)
    {
        pcgScript.minTrapCount = Mathf.Max(1, pcgScript.minTrapCount + Mathf.RoundToInt(rate) + GameManager.DungeonsCleared);
        pcgScript.maxTrapCount = Mathf.Min(6, pcgScript.maxTrapCount + Mathf.RoundToInt(rate) + GameManager.DungeonsCleared);


        Debug.Log("Increasing the trap amount with rate of " + rate);

        Debug.Log("MinTrapCount: " + pcgScript.minTrapCount);
        Debug.Log("MaxTrapCount: " + pcgScript.maxTrapCount);
    }

    //Decrease the game difficulty by decreasing the amount of traps available in the level.
    private void DecreaseTraps(float rate)
    {
        pcgScript.minTrapCount = Mathf.Max(1, pcgScript.minTrapCount - Mathf.RoundToInt(rate));
        pcgScript.maxTrapCount = Mathf.Max(pcgScript.minTrapCount, pcgScript.maxTrapCount - Mathf.RoundToInt(rate));

        Debug.Log("Decrease the trap amount with rate of " + rate);

        Debug.Log("MinTrapCount: " + pcgScript.minTrapCount);
        Debug.Log("MaxTrapCount: " + pcgScript.maxTrapCount);
    }

    #endregion
}
