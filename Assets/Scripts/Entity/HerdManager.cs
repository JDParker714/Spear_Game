using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdManager : MonoBehaviour
{
    //base herd stuff - spawn and keep track of units
    public GameObject herdUnit;
    public int herdSize;
    public List<Unit> herd = new List<Unit>();

    //target transform - entity
    public Transform target_t;

    //herd attack stuff
    public float herd_attack_rate_max = 2f;
    public float herd_attack_rate_min = 2f;
    public float herd_attack_t;

    //waypoints for raoming
    public List<Transform> waypoints;
    public int waypoint_ind;
    //wether to go ++ or -- with indexes
    public bool isReversed = false;
    //is herd alerted
    public bool alerted = false;

    //get herd center of mass
    public Vector2 herd_center;

    private void Start()
    {
        Start_f();
    }

    private void Start_f()
    {
        if (target_t == null) target_t = WorldManager.Instance.Get_Player_T();
        //get waypoint closest to herd spawn
        float dtheta = 10f;
        if (herdSize > 1) dtheta = 360f / ((float)herdSize - 1f);
        float r = 1.5f;

        int min_waypoint_ind = 0;
        float min_waypoint_dist = Mathf.Infinity;
        for (int i = 0; i < waypoints.Count; i++)
        {
            float dist = Vector2.Distance(waypoints[i].position, transform.position);
            if (dist < min_waypoint_dist)
            {
                min_waypoint_dist = dist;
                min_waypoint_ind = i;
            }
        }
        waypoint_ind = min_waypoint_ind;

        //spawn herd
        for (int i = 0; i < herdSize; i++)
        {
            float theta = (i - 1f) * dtheta;
            theta = theta * Mathf.Deg2Rad;
            Vector2 spawnpos = new Vector2(transform.position.x + r * Mathf.Cos(theta),
                transform.position.y + r * Mathf.Sin(theta));
            if (i == 0) spawnpos = transform.position;

            GameObject unit_obj = Instantiate(herdUnit, spawnpos, Quaternion.identity, transform);
            unit_obj.transform.SetParent(null);
            Unit herd_unit = unit_obj.GetComponent<Unit>();
            if (herd_unit != null)
            {
                herd_unit.entity_t = target_t;
                if (waypoints.Count > 0) herd_unit.waypoint_t = waypoints[waypoint_ind];
                herd_unit.manager = this;
                herd_unit.Set_Attack_Rate(herd_attack_rate_max);
                herd.Add(herd_unit);
            }
        }

        herd_attack_t = herd_attack_rate_max;
    }

    //when herd unit reaches waypoint and wants to get next one
    //set next waypoint for entire herd
    public void Next_Waypoint()
    {
        if(!isReversed) waypoint_ind = (waypoint_ind + 1) % waypoints.Count;
        else waypoint_ind = (waypoint_ind - 1 + waypoints.Count) % waypoints.Count;
        foreach (Unit unit in herd)
        {
            unit.waypoint_t = waypoints[waypoint_ind];
        }
    }

    //if a unit of the herd dies - remove from list
    public void HerdDeath(Unit unit)
    {
        if (herd.Contains(unit))
        {
            herd.Remove(unit);
        }
        if(herd.Count<1) Start_f();
    }

    //manage herd attacks
    //if true then a memeber of the herd can attack
    public bool Check_Attack()
    {
        if(herd_attack_t <= 0)
        {
            if (herdSize > 1) herd_attack_t = Mathf.Lerp(herd_attack_rate_max, herd_attack_rate_min, (herd.Count - 1) / (herdSize - 1));
            else herd_attack_t = herd_attack_rate_max;
            return true;
        }
        return false;
    }

    //alert everyone in the herd
    public void Alert()
    {
        alerted = true;
        foreach(Unit unit in herd)
        {
            unit.alerted = true;
        }
    }

    //check if herd is unalerted
    public void UnAlert()
    {
        //check if they are all not alert
        bool all_chil = true;
        foreach (Unit unit in herd)
        {
            if (unit.alerted)
            {
                all_chil = false;
                break;
            }
        }
        //set alert to false and find nearest waypoint
        if (all_chil)
        {
            alerted = false;
            foreach (Unit unit in herd)
            {
                unit.UnAlert();
            }

            int min_waypoint_ind = 0;
            float min_waypoint_dist = Mathf.Infinity;
            for (int i = 0; i < waypoints.Count; i++)
            {
                float dist = Vector2.Distance(waypoints[i].position, herd_center);
                if (dist < min_waypoint_dist)
                {
                    min_waypoint_dist = dist;
                    min_waypoint_ind = i;
                }
            }
            waypoint_ind = min_waypoint_ind;

            foreach (Unit unit in herd)
            {
                unit.waypoint_t = waypoints[waypoint_ind];
            }
        }
    }

    //return herd center
    public Vector2 GetHerdCenter()
    {
        return herd_center;
    }

    public void SetHerdTarget(Transform t)
    {
        target_t = t;
        foreach(Unit unit in herd)
        {
            unit.entity_t = target_t;
        }
    }

    public Transform GetHerdTarget()
    {
        return target_t;
    }

    private void Update()
    {
        //handle herd attack timer
        if (herd_attack_t > 0)
        {
            herd_attack_t -= Time.deltaTime;
        }

        //calculate herd center
        Vector2 pos = Vector2.zero;
        foreach (Unit unit in herd)
        {
            pos = pos + (Vector2)unit.transform.position;
        }

        herd_center = pos / herd.Count;
    }
}
