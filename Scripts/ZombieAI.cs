using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//This script utilises Fuzzy Logic algorithms to have smarter zombies AI enemies.

public class ZombieAI : MonoBehaviour
{
    public float moveSpeed;
    public float followDistance = 10f;
    public int damageAmount = 20;
    public int maxHealth = 50;
    public bool isFollowing = false;

    //Fuzzy logic output
    public float aggressionLevel;

    private GameObject player;
    public int currentHealth;

    private NavMeshAgent navMeshAgent;

    private Animator zombieAnimator;

    private void Start()
    {
        zombieAnimator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(SetHealth());

        damageAmount = 20;
        navMeshAgent = GetComponent<NavMeshAgent>();
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
            navMeshAgent.SetDestination(player.transform.position);
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
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
        Player playerScript = player.gameObject.GetComponent<Player>();
        Debug.Log("Zombie died.");
        GameManager.playerKills++;
        Destroy(gameObject);
    }

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
