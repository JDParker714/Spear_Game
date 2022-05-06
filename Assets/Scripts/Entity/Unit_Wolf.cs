using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Wolf : Unit
{
    #region Unit Dependent Variables
    //chase state
    [SerializeField]
    private float unit_w_trigger_range = 6f;
    [SerializeField]
    private float unit_w_chase_speed = 3f;
    [SerializeField]
    private float unit_w_rand = 1f;
    [SerializeField]
    private float unit_w_lerp = 0.5f;
    //encircle state
    [SerializeField]
    private float unit_w_circle_range = 4f;
    [SerializeField]
    private float unit_w_circle_speed = 4f;
    //retreat state
    [SerializeField]
    private float unit_w_retreat_range = 2f;
    [SerializeField]
    private float unit_w_retreat_speed = 3f;
    //attack state
    [SerializeField]
    private float unit_w_attack_speed = 4.5f;
    #endregion

    protected override void Start()
    {
        Entity_Start();
        //initialize rays
        Init_Rays();
        Init_Vals();
    }

    protected override void Init_Vals()
    {
        //initialize the rest
        state = EntityState.Normal;
        randomVal = Random.Range(-1f * unit_w_rand, unit_w_rand);
        attack_t = Random.Range(0, attack_max_t);
        target_move_speed = 0f;
        move_speed = target_move_speed;
        unit_w_circle_range += randomVal;
        unit_w_retreat_range += randomVal;

        tps_dur = 1f / ticks_per_second;
        tps_t = Random.Range(0f, tps_dur);
    }

    #region State Dependant
    //transition from one state to another
    protected override void Transition_States(EntityState from, EntityState to)
    {
        if (from == to) return;

        switch (from)
        {
            default:
                break;
        }
        switch (to)
        {
            default:
                break;
        }
        state = to;
    }
    //called once an update to update our state machine dependent variables
    //also set target speed
    protected override void Update_State()
    {
        //handle state transitions & once per loop functions

        //if target is null get - one, if that fails return
        target_t = GetStateTarget();

        Vector2 t = target_t.position - transform.position;
        float dist = t.magnitude;
        //smooth look dir - avoid flipping
        look_dir = Mathf.MoveTowards(look_dir, Mathf.Sign(last_move_ray.x), smooth_speed_look * Time.deltaTime);

        //check if it can attack
        if (manager != null)
        {
            if (attack_t <= 0) can_attack = manager.Check_Attack();
            else can_attack = false;
        }
        else
        {
            if (attack_t <= 0) can_attack = true;
        }

        switch (state)
        {
            case EntityState.Normal:
                //ALERT stuff
                //entity t needs to not be null to be in chase mode
                if (entity_t != null)
                {
                    //if alerted just start chase
                    if (alerted)
                    {
                        Transition_States(state, EntityState.Chase);
                    }
                    else
                    {
                        //if not alerted - alert herd manager if close to enemey
                        if (Vector2.Distance(entity_t.position, transform.position) < unit_w_trigger_range && Has_Sight_Line(entity_t))
                        {
                            //alert
                            if (manager != null) manager.Alert();
                            else alerted = true;
                            Transition_States(state, EntityState.Chase);
                        }
                    }
                }
                //look for new targets
                if (entity_t == null || !alerted)
                {
                    Update_Target_Entity();
                }
                //speed and dir
                target_move_speed = unit_w_chase_speed * 0.8f;
                char_t.localScale = new Vector3(Mathf.Sign(look_dir), 1f, 1f);
                break;
            case EntityState.Chase:
                //ALERT STUFF
                //if entity is dead go back to normal
                if (entity_t == null)
                {
                    alerted = false;
                    Transition_States(state, EntityState.Normal);
                }
                //if its too far away - de alert
                else if (alerted && (Vector2.Distance(entity_t.position, transform.position) > unit_w_trigger_range * 1.33f))
                {
                    //unalert
                    if (manager != null)
                    {
                        alerted = false;
                        manager.UnAlert();
                    }
                    else
                    {
                        alerted = false;
                        Transition_States(state, EntityState.Normal);
                    }
                }
                //if we get back in range alert
                else if (!alerted && Vector2.Distance(entity_t.position, transform.position) < unit_w_trigger_range)
                {
                    alerted = true;
                }

                //if we are near enough to the player - prep attack
                //attack stuff
                if (t.magnitude < unit_w_circle_range + unit_w_lerp)
                {
                    if (attack_t > 0) attack_t -= Time.deltaTime;
                }
                else
                {
                    if (attack_t < attack_max_t) attack_t += Time.deltaTime;
                }
                if (attack_t <= 0 && can_attack && !is_attacking)
                {
                    is_attacking = true;
                }
                //movespeed
                if (attack_t <= 0 && is_attacking)
                {
                    target_move_speed = unit_w_attack_speed;
                    if (dist < 1f)
                    {
                        //start attack
                        wait_t = wait_max_t;
                        Transition_States(state, EntityState.Attack);
                        attack_t = attack_max_t;
                        is_attacking = false;
                        //do the thing
                        if (target_t.GetComponent<Entity>() != null)
                        {
                            target_t.GetComponent<Entity>().Take_Damage(1, this);
                        }
                    }
                }
                else if (dist < unit_w_retreat_range + unit_w_lerp / 2f)
                {
                    target_move_speed = unit_w_circle_speed;
                }
                else
                {
                    target_move_speed = unit_w_chase_speed;
                }
                char_t.localScale = new Vector3(Mathf.Sign(look_dir), 1f, 1f);
                break;
            case EntityState.Attack:
                wait_t -= Time.deltaTime;
                target_move_speed = 0;
                move_speed = 0;
                if (wait_t <= 0)
                {
                    Transition_States(state, EntityState.Chase);
                    move_speed = unit_w_retreat_speed;
                }
                char_t.localScale = new Vector3(Mathf.Sign(t.x), 1f, 1f);
                break;
            default:
                Transition_States(state, EntityState.Normal);
                break;
        }
    }

    public override void UnAlert()
    {
        alerted = false;
        Transition_States(state, EntityState.Normal);
    }

    //function called multiple times each step to apply state dependent shaping functions
    protected override float Apply_State_Shaping(float dot, float dist, float local_min, float separation, float cohesion, float center_dist)
    {
        //handle state dependent shaping functions
        float ddot = 0f;

        float a_dot = (dot + 1f) / 2f;
        float s_dot = 1f - Mathf.Abs(dot);
        float r_dot = 1f - a_dot;

        switch (state)
        {
            case EntityState.Normal:
                ddot = a_dot;
                return ddot + local_min + separation + cohesion;
            case EntityState.Chase:
                //if can attack and is attacking -> approach
                if (attack_t <= 0 && is_attacking)
                {
                    ddot = a_dot;
                }
                //if dist is less than our range to retreat -> retreat
                else if (dist < unit_w_retreat_range + unit_w_lerp / 2f)
                {
                    float a = Mathf.Clamp01((dist - unit_w_retreat_range + unit_w_lerp / 2f) / (unit_w_lerp));
                    ddot = Mathf.Lerp(r_dot, s_dot, a);
                }
                //dist is greater than retreat range - lerp between chase and cirle
                //if dist is from retreat_range+lerp/2 -> circle_Range+lerp/2 -> circle
                //else -> chase
                else if (dist < unit_w_circle_range + unit_w_lerp / 2f)
                {
                    float b = Mathf.Clamp01((dist - unit_w_circle_range + unit_w_lerp / 2f) / (unit_w_lerp));
                    ddot = Mathf.Lerp(s_dot, a_dot, b);
                }
                else
                {
                    ddot = a_dot;
                }

                return ddot + local_min + separation;
            default:
                break;
        }
        return r_dot + local_min + separation;
    }

    //decide between whether to target entity or waypoint
    protected override Transform GetStateTarget()
    {
        switch (state)
        {
            case EntityState.Normal:
                if (waypoint_t == null) return transform;
                return waypoint_t;
            case EntityState.Chase:
                if (entity_t == null)
                    if (waypoint_t == null) return transform;
                    else return waypoint_t;
                else return entity_t;
            case EntityState.Attack:
                if (entity_t == null)
                    if (waypoint_t == null) return transform;
                    else return waypoint_t;
                else return entity_t;
            default:
                if (waypoint_t == null) return transform;
                return waypoint_t;
        }
    }

    //look around yourself to find an entity to target
    protected override void Update_Target_Entity(string target = "")
    {
        //find entities within range
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, unit_w_trigger_range, entity_mask);
        float min_dist = Mathf.Infinity;
        Transform pos = null;
        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.GetComponent<Entity>() != null && neighbor.GetComponent<Entity>().entity_team != entity_team)
            {
                if (target == "" || neighbor.GetComponent<Entity>().entity_team == target)
                {
                    float dist = Vector2.Distance(transform.position, neighbor.transform.position);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        pos = neighbor.transform;
                    }
                }
            }
        }

        //we found an entity
        if (pos != null)
        {
            entity_t = pos;
            if (manager != null) manager.SetHerdTarget(pos);
        }
    }
    #endregion
}
