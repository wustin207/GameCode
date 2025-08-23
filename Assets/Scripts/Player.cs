using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//This script contains everything related to the player, such as health, the amount of kills, deaths etc..

public class Player : MonoBehaviour
{
    #region References
    [Header("References")]
    private GameObject dungeonGenerator;
    public SuccessRatioDisplay successRatio;
    public InputActionAsset inputActionAsset;
    #endregion

    #region Health
    [Header("Health")]
    public int maxHealth;
    public int currentHealth;
    public int increaseHealth;
    public Slider healthBar;
    public static bool canTakeDamage;
    public float damageCooldownDuration = 1f;
    #endregion

    #region Kill/Death Stat
    [Header("Kill/Death Stat")]
    public float killDeathRatio;
    public Text killDeathRatioText;
    #endregion

    #region UI & Visuals
    [Header("UI & Visuals")]
    public Image fadePanel;
    private float fadeDuration = 5f;

    private List<Renderer> renderersToFlash = new List<Renderer>();
    private List<Color[]> originalColors = new List<Color[]>();
    private Coroutine flashCoroutine;
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    #endregion


    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public int CurrentHealth
    {
        get { return currentHealth; }
    }

    public int IncreaseHealth
    {
        get { return increaseHealth; }
    }

    public int PlayerKills
    {
        get { return GameManager.playerKills; }
    }

    public int PlayerDeaths
    {
        get { return GameManager.playerDeaths; }
    }

    //Gets the radius of the player
    public float PlayerRadius
    {
        get
        {
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider is CapsuleCollider capsuleCollider)
                return capsuleCollider.radius;
            else if (playerCollider is SphereCollider sphereCollider)
                return sphereCollider.radius;
            else
                return 0f;
        }
    }

    void Start()
    {
        canTakeDamage = true;

        //Every time the player dies, the player spawns with an extra 5 health
        int additionalHealth = GameManager.playerDeaths * 5;
        maxHealth += additionalHealth;
        currentHealth = maxHealth;
        GameObject healthBarObject = GameObject.Find("Health_Bar");

        //Health bar to reflect the current health of the player
        if (healthBarObject != null)
        {
            healthBar = healthBarObject.GetComponent<Slider>();
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        //Get the arm of the player so that the arm can change the color to green or red.
        GameObject Arm = GameObject.Find("SK_FP_CH_Default_Root");
        Renderer[] allRenderers = Arm.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in allRenderers)
        {
            renderersToFlash.Add(rend);

            Material[] newMats = new Material[rend.materials.Length];
            Color[] colorArray = new Color[rend.materials.Length];

            for (int i = 0; i < rend.materials.Length; i++)
            {
                newMats[i] = new Material(rend.materials[i]);
                colorArray[i] = newMats[i].color;
            }

            rend.materials = newMats;
            originalColors.Add(colorArray);
        }

        GameObject dungeonObject = GameObject.FindGameObjectWithTag("dungeon");
        GameObject successRatioObject = GameObject.Find("SuccessRatio");
        GameObject KDRatio = GameObject.Find("KDRatio");
        GameObject Health = GameObject.Find("Health");

        if (dungeonObject == null)
        {
            Debug.LogError("Dungeon not found");
        }
        else
        {
            PCG pcgScript = dungeonObject.GetComponent<PCG>();
            dungeonGenerator = dungeonObject;
        }

        if (fadePanel == null)
        {
            GameObject fadeObj = GameObject.Find("Fade");
            if (fadeObj != null)
            {
                fadePanel = fadeObj.GetComponent<Image>();
                Color c = fadePanel.color;
                c.a = 0f;
                //Make it transperant
                fadePanel.color = c;
            }
            else
            {
                Debug.LogWarning("The Fade Panel has not been found in the scene");
            }
        }
    }

    public void Update()
    {

        //Check if the player Y coordinate is less than -4
        //This is to ensure that when the player is under a map, they die.
        if (transform.position.y < -4f && canTakeDamage == true)
        {
            StartCoroutine(Die());
        }
        killDeathRatio = (float)PlayerKills / Mathf.Max(1, PlayerDeaths);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If the player touches a spike trap, they get damaged for 40 health.
        if (collision.gameObject.CompareTag("Spike"))
        {
                TakeDamage(40);
        }
    }

    //If the player gets damaged, their color flashes red
    private IEnumerator FlashRed()
    {
        foreach (Renderer rend in renderersToFlash)
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderersToFlash.Count; i++)
        {
            Renderer rend = renderersToFlash[i];
            Color[] original = originalColors[i];

            for (int j = 0; j < rend.materials.Length; j++)
            {
                rend.materials[j].color = original[j];
            }
        }
    }

    //If the player gets health back, their color flashes green
    private IEnumerator FlashGreen()
    {
        foreach (Renderer rend in renderersToFlash)
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = Color.green;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderersToFlash.Count; i++)
        {
            Renderer rend = renderersToFlash[i];
            Color[] original = originalColors[i];

            for (int j = 0; j < rend.materials.Length; j++)
            {
                rend.materials[j].color = original[j];
            }
        }
    }

    //Player takes damage
    public void TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            currentHealth -= damage;

            if (healthBar != null)
            {
                healthBar.value = currentHealth;
            }

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRed());

            if (currentHealth <= 0)
            {
                StartCoroutine(Die());
            }
            else
            {
                StartCoroutine(DamageCooldown());
            }
        }
    }

    public IEnumerator Die()
    {
        canTakeDamage = false;
        //Disable any input key when the player dies
        inputActionAsset.Disable();
        GameManager.playerDeaths++;
        GameManager.DungeonsFailed++;
        Debug.Log("Player has died." + " " + "With a total amount of " + GameManager.playerDeaths);

        //Reset the player's health
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        //Fade the black to ensure smooth transition to Lose scene
        yield return StartCoroutine(FadeToBlack());
        yield return new WaitForSeconds(1f);
        canTakeDamage = true;
        inputActionAsset.Enable();
        SceneManager.LoadScene("Lose");
    }

    public IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        Color c = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 1f;
        fadePanel.color = c;
    }

    //Turns off the ability for the player to take damage for a few seconds
    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldownDuration);
        canTakeDamage = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject pickupUIObject = GameObject.FindGameObjectWithTag("PickupUI");

        //If the player touches the health game objects, their current health increases
        if (other.CompareTag("Health"))
        {
            if (currentHealth < maxHealth)
            {
                currentHealth = currentHealth + increaseHealth;

                if (healthBar != null)
                {
                    healthBar.value = currentHealth;
                }

                if (pickupUIObject != null)
                {
                    Text pickupText = pickupUIObject.GetComponent<Text>();

                    if (pickupText != null)
                    {
                        pickupText.text = "Health increased: " + increaseHealth;
                        //Start a coroutine to hide the text after 2 seconds
                        StartCoroutine(HidepickupTextUI(pickupText, 2f));
                    }
                    else
                    {
                        Debug.LogError("TextMeshPro component not found on the object with the tag 'PickupUI'.");
                    }
                }
                else
                {
                    Debug.LogError("Game object with the tag 'PickupUI' not found.");
                }

                if (flashCoroutine != null)
                {
                    StopCoroutine(flashCoroutine);
                }
                flashCoroutine = StartCoroutine(FlashGreen());
                Destroy(other.gameObject);
            }
            else
            {
                Debug.Log("Health is full");
            }
        }
        //If the player touches the portal game object, it will check if there are any enemies remaining, if not then generate the map again
        else if (other.CompareTag("Portal"))
        {
            if (!dungeonGenerator.GetComponent<PCG>().AreEnemiesRemaining())
            {
                Debug.Log("Player has entered the portal");

                if (dungeonGenerator != null)
                {
                    GameManager.RoomsCleared++;
                    dungeonGenerator.GetComponent<PCG>().DestroyRoom();
                    dungeonGenerator.GetComponent<PCG>().GenerateMap();
                }
                else
                {
                    Debug.LogError("DungeonGenerator script reference is not set.");
                }
            }
            else
            {
                Text pickupText = pickupUIObject.GetComponent<Text>();

                if (pickupText != null)
                {
                    pickupText.text = "You need to defeat all enemies first.";
                    //Start a coroutine to hide the text after 2 seconds
                    StartCoroutine(HidepickupTextUI(pickupText, 2f));
                }
                else
                {
                    Debug.LogError("TextMeshPro component not found on the object with the tag 'PickupUI'.");
                }
            }
        }
    }

    //Method to hide the pick up UI text from the screen after a few seconds
    private IEnumerator HidepickupTextUI(Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.text = "";
    }

}
