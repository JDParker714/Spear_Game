using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_AnimEvents : MonoBehaviour
{
    Entity entity;

    private void Start()
    {
        if (entity == null) entity = GetComponentInParent<Entity>();
    }

    public void DoAttack()
    {
        if (entity != null) entity.Do_Attack();
    }
}
