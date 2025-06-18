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
    private GameObject dungeonGenerator;
    public SuccessRatioDisplay successRatio;
    public InputActionAsset inputActionAsset;

    public int maxHealth;
    public int currentHealth;
    public int increaseHealth;
    public static bool canTakeDamage = true;
    public float damageCooldownDuration = 1f;
    public float killDeathRatio;
    public Text killDeathRatioText;
    public Text HealthText;

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
        int additionalHealth = GameManager.playerDeaths * 5;
        maxHealth += additionalHealth;
        currentHealth = maxHealth;

        canTakeDamage = true;

        GameObject dungeonObject = GameObject.FindGameObjectWithTag("dungeon");
        GameObject successRatioObject = GameObject.Find("SuccessRatio");
        GameObject KDRatio = GameObject.Find("KDRatio");
        GameObject Health = GameObject.Find("Health");

        if (dungeonObject == null)
        {
            Debug.LogError("Cannot find the DungeonGenerator GameObject with the tag 'dungeon'. Make sure it is in the scene and properly tagged.");
        }
        else
        {
            PCG pcgScript = dungeonObject.GetComponent<PCG>();

            successRatio = successRatioObject.GetComponent<SuccessRatioDisplay>();
            HealthText = Health.GetComponent<Text>();
            killDeathRatioText = KDRatio.GetComponent<Text>();

            dungeonGenerator = dungeonObject;
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
        killDeathRatioText.text = "Kill/Death Ratio: " + killDeathRatio.ToString("F2");

        HealthText.text = "Health: " + currentHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
                TakeDamage(40);
        }
    }

    public void TakeDamage(int damage)
    {
        if (canTakeDamage)
        {
            currentHealth -= damage;
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
        successRatio.UpdateSuccessRatioText();
        Debug.Log("Player has died." + " " + "With a total amount of " + GameManager.playerDeaths);
        //Reset the player's health
        currentHealth = maxHealth;
        yield return new WaitForSeconds(5f);
        canTakeDamage = true;
        inputActionAsset.Enable();
        SceneManager.LoadScene("Lose");
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldownDuration);
        canTakeDamage = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Health"))
        {
            if (currentHealth < maxHealth)
            {
                currentHealth = currentHealth + increaseHealth;
                GameObject pickupUIObject = GameObject.FindGameObjectWithTag("PickupUI");

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
                Destroy(other.gameObject);
            }
            else
            {
                Debug.Log("Health is full");
            }
        }
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
                Debug.Log("Cannot progress to another dungeon. Clear all enemies first.");
            }
        }
    }

    private IEnumerator HidepickupTextUI(Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.text = "";
    }

}
