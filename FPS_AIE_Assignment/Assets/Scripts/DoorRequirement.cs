using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRequirement : MonoBehaviour, IDamageable
{
    public void TakeDamage(float damage)
    {
        print("damaged");
        gameObject.SetActive(false);
    }

    public void OnDeath()
    {

    }
}
