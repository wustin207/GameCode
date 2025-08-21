using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private int damage;

    public void Start()
    {
        //Destroy arrow after 3 seconds for performance.
        Destroy(gameObject, 4f);
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }
}

