using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//This script utilises Fuzzy Logic algorithms to have smarter zombies AI enemies.

public class Zombie : MonoBehaviour
{
    #region Movement
    [Header("Movement")]
    public float moveSpeed;
    public float followDistance = 10f;
    public bool isFollowing = false;
    #endregion

    #region Health
    [Header("Health")]
    public int damageAmount;
    public int maxHealth = 50;
    public int currentHealth;
    #endregion

    #region Fuzzy Logic
    [Header("Fuzzy Logic")]
    //Fuzzy logic output
    public float aggressionLevel;
    #endregion

    #region Player & Zombie
    [Header("Player & Zombie")]
    private GameObject player;
    private NavMeshAgent navMeshAgent;
    private Animator zombieAnimator;
    #endregion

    #region Visuals
    [Header("Visuals")]
    private List<Material> originalMaterials = new List<Material>();
    private List<Renderer> renderersToFlash = new List<Renderer>();
    private Coroutine flashCoroutine;
    public float flashDuration = 4.1f;
    #endregion

    #region Audio
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    #endregion

    private void Start()
    {
        zombieAnimator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(SetHealth());

        damageAmount = 20;
        navMeshAgent = GetComponent<NavMeshAgent>();


        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
          foreach (Renderer renderer in childRenderers)
            {
              renderersToFlash.Add(renderer);
              //Cloning the original materials to rever colors
              foreach (var mat in renderer.materials)
                {
                  originalMaterials.Add(new Material(mat));
                }
            }
        }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        //Calculate move speed based on player distance
        CalculateMoveSpeedLevelDistance(distanceToPlayer);

        //Calculate aggression level based on health
        float healthAggression = CalculateAggressionLevelHealth();

        //Combine aggression levels from fuzzy logic and health
        aggressionLevel = Mathf.Max(aggressionLevel, healthAggression);

        if (distanceToPlayer <= followDistance)
        {
            isFollowing = true;
        }
        else
        {
            navMeshAgent.ResetPath();
        }

        if (isFollowing)
        {
            //follow the player using NavMesh
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 targetPosition = player.transform.position + randomOffset;
            navMeshAgent.SetDestination(targetPosition);

        }
    }

    //This method is being called as an event in the Animation so that the sound always plays at the exact animation.
    public void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }


    private IEnumerator SetHealth()
    {
        yield return new WaitForSeconds(1f);
        currentHealth = maxHealth;
    }

    #region Fuzzy Logic

    private void CalculateMoveSpeedLevelDistance(float distance)
    {
        //Calculate move speed based on Player Distance
        float closeMembership = CalculateMembership(distance, 0f, 1f, 3f);
        float mediumMembership = CalculateMembership(distance, 1f, 3f, 6f);
        float farMembership = CalculateMembership(distance, 3f, 6f, 10f);

        //Calculate a modified moveSpeed based on distance and membership
        float closeMoveSpeed = 2.8f;
        float mediumMoveSpeed = 3f;
        float farMoveSpeed = 3.5f;

        //Weighted average to adjust moveSpeed based on distance and membership
        moveSpeed = Mathf.Lerp(moveSpeed, closeMoveSpeed, closeMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, mediumMoveSpeed, mediumMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, farMoveSpeed, farMembership);

        navMeshAgent.speed = moveSpeed;

        //When the membership is close, the zombie starts attacking
        if (closeMembership > mediumMembership && closeMembership > farMembership)
        {
            zombieAnimator.SetBool("Walk", false);
            zombieAnimator.SetBool("Run", false);
            zombieAnimator.SetBool("Idle", false);
            zombieAnimator.SetBool("Attack", true);
        }
        //When the membership is medium, the zombie starts walking
        else if (mediumMembership > closeMembership && mediumMembership > farMembership)
        {
            zombieAnimator.SetBool("Walk", true);
            zombieAnimator.SetBool("Run", false);
            zombieAnimator.SetBool("Idle", false);
            zombieAnimator.SetBool("Attack", false);
        }
        //When the membership is far, the zombie starts running
        else
        {
            if (isFollowing)
            {
                zombieAnimator.SetBool("Walk", false);
                zombieAnimator.SetBool("Run", true);
                zombieAnimator.SetBool("Idle", false);
                zombieAnimator.SetBool("Attack", false);
            }
        }

    }

    //Calculate aggression based on health
    private float CalculateAggressionLevelHealth()
    {
        float aggression = Mathf.Clamp01(1f - (float)currentHealth / maxHealth);
        return aggression;
    }

    private float CalculateMembership(float value, float a, float b, float c)
    {
        return Mathf.Clamp01((Mathf.Min(value - a, c - value)) / (b - a));
    }

    #endregion

    //Method so that this enemy flashes red whenever they get damaged
    private IEnumerator FlashRed()
    {
        // Set all renderer materials to red
        foreach (Renderer renderer in renderersToFlash)
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = Color.red;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Revert materials to original color
        int materialIndex = 0;
        foreach (Renderer renderer in renderersToFlash)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (materialIndex < originalMaterials.Count)
                {
                    renderer.materials[i].color = originalMaterials[materialIndex].color;
                    materialIndex++;
                }
            }
        }
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRed());
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isFollowing = true;
        }
    }

    private void Die()
    {
        //Increase the player kills amount
        GameManager.playerKills++;
        //Destroy this game object
        Destroy(gameObject);
    }

    //When colliding with the player, start damaging the player
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                //The amount of damage the zombie hits depends on the membership state
                int adjustedDamage = damageAmount + Mathf.RoundToInt(damageAmount * aggressionLevel);
                playerScript.TakeDamage(adjustedDamage);
            }
        }
    }
}
