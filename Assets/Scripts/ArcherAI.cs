using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//This script utilises Fuzzy Logic algorithms to have smarter archers AI enemies.

public class ArcherAI : MonoBehaviour
{
    #region Movement & AI
    [Header("Movement & AI")]
    public float moveSpeed;
    public float followDistance = 10f;
    public bool isFollowing = false;

    //Fuzzy logic output
    public float aggressionLevel;
    #endregion

    #region Combat
    [Header("Combat")]
    public int damageAmount;
    public int maxHealth = 50;
    public int currentHealth;

    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;
    private float attackTimer = 0f;
    #endregion

    #region Components
    [Header("Components")]
    private GameObject player;
    private NavMeshAgent navMeshAgent;
    private Animator archerAnimator;
    #endregion

    #region Audio
    [Header("Audio")]
    [SerializeField] private AudioClip shootAudioClip;
    public AudioSource audioSource;
    #endregion

    #region Visual Effects
    [Header("Visual Effects")]
    private List<Renderer> renderersToFlash = new List<Renderer>();
    private List<Color[]> originalColors = new List<Color[]>();
    private Coroutine flashCoroutine;
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    #endregion


    private void Start()
    {
        archerAnimator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(SetHealth());

        firePoint = transform.Find("FirePoint");

        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not found.");
        }

        //Enemy flashes red when they are damaged
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
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

        damageAmount = 20;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (player == null) return;

        //Calculate distance between this enemy to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        //Calculate move speed based on player distance
        CalculateMoveSpeedLevelDistance(distanceToPlayer);

        //Calculate aggression level based on health of this enemy
        float healthAggression = CalculateAggressionLevelHealth();

        //Combine aggression levels from fuzzy logic and health
        aggressionLevel = Mathf.Max(aggressionLevel, healthAggression);

        attackTimer += Time.deltaTime;

        //If the distance calculation is less than the follow distance, this enemy starts following the player
        if (distanceToPlayer <= followDistance)
        {
            isFollowing = true;
        }
        else
        {
            navMeshAgent.ResetPath();
        }

        if (isFollowing && !archerAnimator.GetBool("Attack"))
        {
            //follow the player using NavMesh
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 2f), 0, Random.Range(-1f, 2f));
            Vector3 targetPosition = player.transform.position + randomOffset;
            navMeshAgent.SetDestination(targetPosition);

        }

    }

    //This method is being called as an event in the Animation so that the arrow always shoots at the exact animation.
    private void Shoot()
    {
        //Shooting animation
        if (attackTimer >= attackCooldown)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1f);

            archerAnimator.SetBool("Attack", true);
            attackTimer = 0f;
            if (arrowPrefab != null && firePoint != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = arrow.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = firePoint.forward * 15f;
                }

                int adjustedDamage = damageAmount + Mathf.RoundToInt(damageAmount * aggressionLevel);

                Arrow arrowScript = arrow.GetComponentInChildren<Arrow>();
                if (arrowScript != null)
                {
                    arrowScript.SetDamage(adjustedDamage);
                }
            }
        }
    }

    //This method is being called as an event in the Animation so that the sound always plays at the exact animation.
    public void PlayShootSound()
    {
        if (shootAudioClip != null)
        {
            audioSource.PlayOneShot(shootAudioClip);
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
        //Fuzzy logic gradually calculates the distance between this enemy to the player.
        float closeMembership = CalculateMembership(distance, 0f, 1f, 5f);
        float mediumMembership = CalculateMembership(distance, 5f, 9f, 14f);
        float farMembership = CalculateMembership(distance, 14f, 19f, 22f);

        //Calculate a modified moveSpeed based on distance and membership
        float closeMoveSpeed = 0.8f;
        float mediumMoveSpeed = 1.5f;
        float farMoveSpeed = 2.5f;

        //Weighted average to adjust moveSpeed based on distance and membership
        moveSpeed = Mathf.Lerp(moveSpeed, closeMoveSpeed, closeMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, mediumMoveSpeed, mediumMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, farMoveSpeed, farMembership);
        navMeshAgent.speed = moveSpeed;

        //When the membership is close or medium, the Archer starts Attacking
        if ((closeMembership > mediumMembership && closeMembership > farMembership) ||
            (mediumMembership > closeMembership && mediumMembership > farMembership))
        {
            navMeshAgent.ResetPath();

            archerAnimator.SetBool("Walk", false);
            archerAnimator.SetBool("Idle", false);
            archerAnimator.SetBool("Attack", true);
        }
        else
        {
            //In far range walk
            if (isFollowing)
            {
                archerAnimator.SetBool("Walk", true);
                archerAnimator.SetBool("Idle", false);
                archerAnimator.SetBool("Attack", false);

            }
        }
    }



    private float CalculateAggressionLevelHealth()
    {
        //Calculate aggression based on health
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


    public void TakeDamage(int damage)
    {

            currentHealth -= damage;

            //Stopping previous flash and start a new one
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
}
