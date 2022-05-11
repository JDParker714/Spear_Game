using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity
{
    //base entity stuff
    protected enum EntityState
    {
        Normal,
        Chase,
        Attack
    }
    [SerializeField]
    protected EntityState state;

    //raycast stuff
    [Header("Raycast Stuff")]
    public int num_rays = 8;
    protected List<AI_Ray> raycasts = new List<AI_Ray>();
    public float sight_range_obj = 3f;
    public float sight_range_entity = 3f;
    public Vector2 unit_size;
    protected Vector2 move_ray;
    protected Vector2 last_move_ray;

    //tick per second stuff
    protected static int visible_tps = 30;
    protected static int invisible_tps = 10;

    protected int ticks_per_second = 30;
    protected float tps_dur;
    protected float tps_t;

    //unit movement and targeting
    [Header("Unit Stuff")]
    public Transform target_t;
    //transform of entity we are fighting or interacting with
    public Transform entity_t;
    //transform of waypoint - static position to go to
    public Transform waypoint_t;
    public float move_speed = 2f;
    protected float target_move_speed;
    //hysteresis
    protected static float smooth_speed_ray = 5f;
    protected static float smooth_speed_move_ray = 5f;
    protected static float smooth_speed_move_speed = 3f;
    protected static float smooth_speed_look = 3f;

    //animation
    public Animator char_anim;
    public Transform char_t;

    //masking
    public LayerMask obj_mask;
    public LayerMask entity_mask;

    //management and inventory
    public HerdManager manager;
    public Item item_drop;

    //state machine values
    [Header("State Stuff")]
    public float local_min_scalar = 0.05f;
    public float entity_avoid_scalar = 0.25f;
    public float entity_cohesion_scalar = 0.25f;
    #region Unit Dependent Variables
    #endregion
    //stay still
    protected float wait_t;
    protected float wait_max_t = 0.25f;
    protected float randomVal;
    //attack variables
    protected bool can_attack;
    protected bool is_attacking = false;
    public float attack_max_t = 5f;
    public GameObject attack_m_effect;
    protected float attack_t;

    protected float look_dir;
    public bool alerted;

    // Start is called before the first frame update
    protected override void Start()
    {
        Entity_Start();
        //initialize rays
        Init_Rays();
        Init_Vals();
    }

    protected void Entity_Start()
    {
        base.Start();
    }

    protected void Init_Rays()
    {
        //initialize rays
        for (int i = 0; i < num_rays; i++)
        {
            float angle = 2 * Mathf.PI * (float)i / (float)num_rays;
            Vector2 ray_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            AI_Ray raycast = new AI_Ray(ray_dir, 0, 1);

            raycasts.Add(raycast);
        }
    }

    protected virtual void Init_Vals()
    {
        //initialize the rest
        state = EntityState.Normal;
        randomVal = Random.Range(-1f * 1f, 1f);
        attack_t = Random.Range(0, attack_max_t);
        target_move_speed = 0f;
        move_speed = target_move_speed;

        tps_dur = 1f / ticks_per_second;
        tps_t = Random.Range(0f, tps_dur);
    }

    #region Visibility
    protected virtual void OnBecameVisible()
    {
        //enabled = true;
        ticks_per_second = visible_tps;
        tps_dur = 1f / ticks_per_second;
    }

    protected virtual void OnBecameInvisible()
    {
        //enabled = false;
        ticks_per_second = invisible_tps;
        tps_dur = 1f / ticks_per_second;
    }
    #endregion

    #region State Dependant
    //transition from one state to another
    protected virtual void Transition_States(EntityState from, EntityState to)
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
    protected virtual void Update_State()
    {
        target_t = GetStateTarget();

        Vector2 t = target_t.position - transform.position;
        float dist = t.magnitude;
        //smooth look dir - avoid flipping
        //Hysteresis 1
        look_dir = Mathf.MoveTowards(look_dir, Mathf.Sign(last_move_ray.x), smooth_speed_look * Time.deltaTime);
        char_t.localScale = new Vector3(Mathf.Sign(look_dir), 1f, 1f);
    }

    public virtual void UnAlert()
    {
        alerted = false;
        Transition_States(state, EntityState.Normal);
    }

    protected virtual bool Has_Sight_Line(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float dist = (target.position - transform.position).magnitude;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obj_mask);
        return hit.collider == null;
    }

    //function called multiple times each step to apply state dependent shaping functions
    protected virtual float Apply_State_Shaping(float dot, float dist, float local_min, float separation, float cohesion, float center_dist)
    {
        //handle state dependent shaping functions
        float ddot = 0f;

        float a_dot = (dot + 1f) / 2f;
        float s_dot = 1f - Mathf.Abs(dot);
        float r_dot = 1f - a_dot;

        ddot = a_dot;

        return ddot + local_min + separation + cohesion;
    }
    //Update Weights
    protected virtual void Update_Weights()
    {
        if (target_t == null) return;

        Vector2 t = target_t.position - transform.position;
        Vector2 t_norm = t.normalized;
        Vector2 max_dir = t.normalized;
        float max_weight = 0;
        float min_obj_weight = Mathf.Infinity;
        int ray_ind = 0;

        //Get obstacle map
        foreach (AI_Ray ray in raycasts)
        {
            RaycastHit2D hit_obj = Physics2D.BoxCast(transform.position, unit_size, 0f, ray.dir, sight_range_obj, obj_mask);
            //RaycastHit2D hit_obj = Physics2D.Raycast(transform.position, ray.dir, sight_range_obj, obj_mask);

            //Do object map
            float obj_weight = 0f;
            if (hit_obj.collider != null)
            {
                obj_weight = sight_range_obj - hit_obj.distance;
            }

            if (obj_weight < min_obj_weight) min_obj_weight = obj_weight;

            ray.obstacle_map = obj_weight;
        }
        //calculate seperation
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, sight_range_entity, entity_mask);
        //get nearest neighbor - list to keep track of unique gameobjects
        List<GameObject> neighbors_obj = new List<GameObject>();
        neighbors_obj.Add(gameObject);
        Vector2 separation = Vector2.zero;
        //calculate cohesion
        Vector2 Cohesion = (manager.GetHerdCenter() - (Vector2)transform.position);
        float center_dist = Cohesion.magnitude;
        Cohesion = Cohesion.normalized * Mathf.Max(0f, Cohesion.magnitude);
        foreach (Collider2D neighbor in neighbors)
        {
            if (!neighbors_obj.Contains(neighbor.gameObject))
            {
                Vector2 dir = transform.position - neighbor.transform.position;
                float dist = Vector2.Distance(transform.position, neighbor.transform.position);
                separation += dir.normalized * (1f - Mathf.Pow(dist / sight_range_entity, 2f));
                neighbors_obj.Add(neighbor.gameObject);
            }
        }
        //get interest map
        foreach (AI_Ray ray in raycasts)
        {
            ray.blocked = ray.obstacle_map > min_obj_weight;

            //float e_dot = (1f - Mathf.Abs(Vector2.Dot(separation.normalized, ray.dir.normalized) - 0.65f));
            float e_dot = Vector2.Dot(separation.normalized, ray.dir.normalized);
            float entity_offset = e_dot * separation.magnitude * entity_avoid_scalar;

            //offset in direction of movement
            float local_min_offset = local_min_scalar * Vector2.Dot(rb.velocity, ray.dir);

            float c_dot = Vector2.Dot(Cohesion, ray.dir.normalized);
            float cohesion_offset = c_dot * entity_cohesion_scalar * Cohesion.magnitude;

            //value of ray along desired path from 0->1
            float dot = Vector2.Dot(t_norm, ray.dir.normalized);
            float move_weight = Apply_State_Shaping(dot, t.magnitude, local_min_offset, entity_offset, cohesion_offset, center_dist);

            //Hysteresis 2
            //smooth values overtime?
            //ray.weight = Mathf.Lerp(ray.last_weight, move_weight, 0.8f);
            ray.weight = move_weight;

            if (ray.weight > max_weight && !ray.blocked)
            {
                max_weight = ray.weight;
                max_dir = ray.dir;
                ray_ind = raycasts.IndexOf(ray);
            }
        }
        //subslot movement - interpolation
        if (num_rays >= 4 && ray_ind >= 0 && ray_ind < raycasts.Count)
        {
            int i0 = (ray_ind - 1 + raycasts.Count) % raycasts.Count;
            int i1 = (ray_ind + raycasts.Count) % raycasts.Count;
            int i2 = (ray_ind + 1 + raycasts.Count) % raycasts.Count;
            int i3 = (ray_ind + 2 + raycasts.Count) % raycasts.Count;

            float x0 = -1f;
            float x1 = 0f;
            float x2 = 1f;
            float x3 = 2f;

            float y0 = raycasts[i0].weight;
            float y1 = raycasts[i1].weight;
            float y2 = raycasts[i2].weight;
            float y3 = raycasts[i3].weight;

            float m01 = (y1 - y0) / (x1 - x0);
            float c01 = (m01 * -1f * x1) + y1;
            float m23 = (y3 - y2) / (x3 - x2);
            float c23 = (m23 * -1f * x3) + y3;

            float y_g = ((c01 * m23) - (c23 * m01)) / ((m01 * -1f) - (m23 * -1f));
            float x_g = ((-1f * c23) - (-1f * c01)) / ((m01 * -1f) - (m23 * -1f));
            if (y_g > max_weight && Mathf.Abs(x_g) < 1)
            {
                if (x_g > 0)
                {
                    max_dir = Vector2.Lerp(raycasts[i1].dir, raycasts[i2].dir, Mathf.Abs(x_g));
                }
                else
                {
                    max_dir = Vector2.Lerp(raycasts[i1].dir, raycasts[i0].dir, Mathf.Abs(x_g));
                }
            }
        }
        //last weight
        foreach (AI_Ray ray in raycasts)
        {
            ray.last_weight = ray.weight;
        }
        //last_move_ray = move_ray;
        move_ray = max_dir;
    }

    //decide between whether to target entity or waypoint
    protected virtual Transform GetStateTarget()
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
    protected virtual void Update_Target_Entity(string target = "")
    {
        //find entities within range
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, 2f, entity_mask);
        float min_dist = Mathf.Infinity;
        Transform pos = null;
        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.GetComponent<Entity>() != null && neighbor.GetComponent<Entity>().entity_team != entity_team)
            {
                if(target == "" ||  neighbor.GetComponent<Entity>().entity_team == target)
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

    #region Attacking
    public virtual void Set_Attack_Rate(float t)
    {
        attack_max_t = t;
        attack_t = Random.Range(0, attack_max_t);
    }
    #endregion

    #region Base Entity
    //can stay the same
    protected override void Death()
    {
        //drop item
        ItemWorld.SpawnItemWorld(transform.position, item_drop);
        if (manager != null)
        {
            manager.HerdDeath(this);
        }
        Destroy(gameObject);
    }
    #endregion

    public override void Take_Damage(int amount, Entity source)
    {
        //dont take damage
        if (source != null) Update_Target_Entity(source.entity_team);

        //stop attackies
        attack_t = attack_max_t;
        is_attacking = false;
        Transition_States(state, EntityState.Chase);

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

    public override void Do_Attack()
    {
        if (attack_target_entity == null) return;

        attack_dir = (target_t.position - transform.position).normalized;
        float angle = Mathf.Atan2(attack_dir.y, attack_dir.x) * Mathf.Rad2Deg;
        Instantiate(attack_m_effect, (Vector2)transform.position, Quaternion.Euler(0, 0, angle + 90));

        Collider2D[] neighbors = Physics2D.OverlapCircleAll((Vector2)transform.position + attack_dir.normalized*1f, 0.5f, entity_mask);
        List<GameObject> neighbors_obj = new List<GameObject>();
        neighbors_obj.Add(gameObject);
        foreach (Collider2D neighbor in neighbors)
        {
            if (!neighbors_obj.Contains(neighbor.gameObject))
            {
                if (neighbor.GetComponent<Entity>() != null && neighbor.GetComponent<Entity>().entity_team != entity_team)
                {
                    neighbor.GetComponent<Entity>().Take_Damage(1, this);
                }
                neighbors_obj.Add(neighbor.gameObject);
            }
        }

        attack_target_entity = null;
        is_attacking = false;
    }

    //can stay the same
    protected override void Update()
    {
        //paused
        if (Time.timeScale == 0) return;

        //tik tok
        tps_t += Time.deltaTime;
        while(tps_t > tps_dur)
        {
            tps_t -= tps_dur;
            Tick();
        }
        //waypoint progression
        if(Vector2.Distance(transform.position, waypoint_t.position) < 0.5f)
        {
            manager.Next_Waypoint();
        }
        Update_Knock();
        Update_State();
        //Hysteresis 3
        move_speed = Mathf.MoveTowards(move_speed, target_move_speed, smooth_speed_move_speed*Time.deltaTime);
        char_anim.SetBool("isRunning", move_speed > 0.5f);
    }
    //can stay the same
    protected virtual void Tick()
    {
        Update_Weights();
    }

    //Physics Step - can stay the same
    protected override void FixedUpdate()
    {
        //Update_Weights();
        //Hysteresis 4
        Vector2 lerp_move_ray = Vector2.MoveTowards(last_move_ray.normalized, move_ray.normalized, smooth_speed_move_ray * Time.fixedDeltaTime);
        rb.velocity = lerp_move_ray * move_speed + knock_dir * knock_speed;
        //rb.velocity = move_ray.normalized * move_speed + knock_dir * knock_speed;
        last_move_ray = lerp_move_ray;
    }

    //doesnt need to be changed
    protected virtual void OnDrawGizmos()
    {
        float max_weight = 0;
        Vector2 max_dir = Vector2.zero;
        float min_weight = Mathf.Infinity;

        foreach (AI_Ray ray in raycasts)
        {
            if (ray.weight > max_weight)
            {
                max_weight = ray.weight;
                max_dir = ray.dir;
            }
            if (ray.weight < min_weight) min_weight = ray.weight;
        }

        foreach (AI_Ray ray in raycasts)
        {
            Color c = Color.Lerp(Color.red, Color.green, (ray.weight - min_weight) / (max_weight - min_weight));
            Debug.DrawRay(transform.position, ray.dir, c);
            Color c_b = ray.blocked ? Color.red : Color.green;
            Gizmos.color = c_b;
            Gizmos.DrawSphere((Vector2)transform.position + ray.dir, 0.1f);
        }
        Debug.DrawRay(transform.position + new Vector3(0.05f, 0.05f, 0f), move_ray, Color.blue);

        Gizmos.color = Color.cyan;
        if(attack_target_entity != null) Gizmos.DrawWireSphere(transform.position + (Vector3)attack_dir.normalized * 1f, 0.5f);
    }
}
public class AI_Ray
{
    public Vector2 dir;
    public float obstacle_map;
    public float weight;
    public float last_weight;
    public bool blocked;

    public AI_Ray(Vector2 _dir, float _ob_map, float _weight)
    {
        dir = _dir;
        obstacle_map = _ob_map;
        weight = _weight;
        last_weight = _weight;
        blocked = true;
    }
}

