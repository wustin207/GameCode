using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//This script utilises Fuzzy Logic algorithms to have smarter skeletons AI enemies.

public class SkeletonAI : MonoBehaviour
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

    private Animator skeletonAnimator;

    private void Start()
    {
        skeletonAnimator = GetComponent<Animator>();
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
        float closeMoveSpeed = 2f;
        float mediumMoveSpeed = 2.7f;
        float farMoveSpeed = 4f;

        //Weighted average to adjust moveSpeed based on distance and membership
        moveSpeed = Mathf.Lerp(moveSpeed, closeMoveSpeed, closeMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, mediumMoveSpeed, mediumMembership);
        moveSpeed = Mathf.Lerp(moveSpeed, farMoveSpeed, farMembership);

        navMeshAgent.speed = moveSpeed;

        //When the membership is close, the skeleton starts attacking
        if (closeMembership > mediumMembership && closeMembership > farMembership)
        {
            skeletonAnimator.SetBool("Walk", false);
            skeletonAnimator.SetBool("Block", false);
            skeletonAnimator.SetBool("Idle", false);
            skeletonAnimator.SetBool("Attack", true);
        }
        //When the membership is medium, the skeleton starts blocking
        else if (mediumMembership > closeMembership && mediumMembership > farMembership)
        {
            skeletonAnimator.SetBool("Walk", false);
            skeletonAnimator.SetBool("Block", true);
            skeletonAnimator.SetBool("Idle", false);
            skeletonAnimator.SetBool("Attack", false);
        }
        //When the membership is far, the skeleton starts walking
        else
        {
            if(isFollowing)
            {
                skeletonAnimator.SetBool("Walk", true);
                skeletonAnimator.SetBool("Block", false);
                skeletonAnimator.SetBool("Idle", false);
                skeletonAnimator.SetBool("Attack", false);
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
        //If the skeleton is blocking, the skeleton can't get damaged
        if(skeletonAnimator.GetBool("Block") == false)
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
        else
        {
            Debug.Log("Can't Damage skeleton is blocking");
        }
    }

    private void Die()
    {
        Player playerScript = player.gameObject.GetComponent<Player>();
        Debug.Log("Skeleton died.");
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
                //The amount of damage the skeleton hits depends on the aggression level
                int adjustedDamage = damageAmount + Mathf.RoundToInt(damageAmount * aggressionLevel);
                playerScript.TakeDamage(adjustedDamage);
            }
        }
    }
}
