using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Entity
{
    public override void Take_Damage(int amount, Entity source)
    {
        Debug.Log("Dummy was hit");

        //knock
        knock_dir = (transform.position - source.transform.position).normalized;
        knock_speed = max_knock_speed;

        Hit_Effect(knock_dir);

        health -= amount;
        if (blood_effect != null) blood_effect.SetActive(health <= max_health / 2);
        if (health <= 0)
        {
            Death();
        }
    }
}
