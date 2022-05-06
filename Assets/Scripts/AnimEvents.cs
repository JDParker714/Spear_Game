using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    PlayerController player;

    private void Start()
    {
        if (player == null) player = GetComponentInParent<PlayerController>();
    }

    public void Do_Attack_R_Event()
    {
        if (player != null) player.Do_Attack_R();
    }

    public void Do_Attack_M_Event()
    {
        if (player != null) player.Do_Attack_M();
    }

    public void Do_Attack_All_Event()
    {
        if (player != null) player.Do_Attack_M();
        if (player != null) player.Do_Attack_R();
    }

    public void End_Attack_R_Event()
    {
        if (player != null) player.End_Attack_R();
    }

    public void End_Attack_M_Event()
    {
        if (player != null) player.End_Attack_M();
    }

    public void End_Attack_All_Event()
    {
        if (player != null) player.End_Attack_M();
        if (player != null) player.End_Attack_R();
    }
}
