using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Entity
{
    public static float player_radius = 0.5f;
    //base stuff
    private enum PlayerState
    {
        Normal,
        Rolling,
        Attacking
    }
    private PlayerState state;
    private Vector2 input_dir;
    private Vector2 last_input_dir;

    [Header("Player Stuff")]
    public Transform char_t;
    public Animator char_anim;
    public float move_speed = 3f;
    private float run_t = 0f;

    public LayerMask obj_mask;
    public LayerMask entity_mask;
    public LayerMask item_mask;

    //dodge roll stuff
    [Header("Dodge Roll")]
    private Vector2 roll_dir;
    public float max_roll_speed = 250f;
    public float roll_speed_mod = 5f;
    public float roll_speed_min = 50f;
    private float roll_speed = 5f;
    public float roll_t = 0f;
    public float roll_cd = 0.5f;
    //test attack stuff
    [Header("Test Melee")]
    public float attack_m_range = 1f;
    public float attack_m_width = 1f;
    public float attack_m_t = 0f;
    public float attack_m_cd = 0.2f;
    private Vector3 attack_mouse_pos;
    public GameObject attack_m_effect;

    [Header("Test Ranged")]
    public GameObject attack_r_prefab;
    public Transform attack_r_origin;
    public float attack_r_range = 5f;
    public float attack_r_speed = 4f;
    public float attack_r_t = 0f;
    public float attack_r_cd = 0.2f;

    public List<string> interruptAnims = new List<string>();

    //Input Buffering
    private const float input_buffer_attack = 0.15f;
    private float ib_attack_0_t = 0;
    private float ib_attack_1_t = 0;
    private const float input_buffer_dodge = 0.15f;
    private float ib_dodge_t = 0;

    [Header("Interact Stuff")]
    public static float interact_range = 1.5f;
    public float item_magnetic_range = 2f;
    public float item_magnetic_force = 100f;
    public Animator key_prompt_anim;
    private List<GameObject> interactable_objs = new List<GameObject>();

    private Inventory inventory;

    private bool can_move = true;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        inventory = new Inventory(transform);
        inventory.AddItem(new Item { amount = 80, itemType = Item.ItemType.Coin });
        inventory.AddItem(new Item { amount = 100, itemType = Item.ItemType.FruitBlueberry });

        StartCoroutine(waitItemSpawn());

        key_prompt_anim.gameObject.SetActive(false);
    }

    public IEnumerator waitItemSpawn()
    {
        yield return new WaitForSeconds(0.15f);

        float r = 2.5f;
        int n = 3;
        float delta = 360f / n;
        for (int i = 0; i < n; i++)
        {
            float theta = delta * i * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
            ItemWorld.SpawnItemWorld((Vector2)transform.position + pos, new Item { amount = Random.Range(1, 5), itemType = (Item.ItemType)Random.Range(0, 3) });
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -1 * Camera.main.transform.position.z));
        return worldPos;
    }

    public void Set_Can_Move(bool val)
    {
        can_move = val;
    }

    private Vector2 Circular_Input(Vector2 input)
    {
        if (input == Vector2.zero) return input;

        float theta = Mathf.Atan2(input.y, input.x);
        float circ_input_magnitude = new Vector2(input.x * Mathf.Cos(theta), input.y * Mathf.Sin(theta)).magnitude;
        Vector2 circ_input_normalized = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        return circ_input_normalized * circ_input_magnitude;
    }

    private bool isAnimInterruptable()
    {
        AnimatorStateInfo state_info = char_anim.GetCurrentAnimatorStateInfo(0);

        foreach(string anim in interruptAnims)
        {
            if (state_info.IsName(anim)) return true;
        }
        return false;
    }

    private void Handle_Inputs()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!MenuManager.isPaused)
            {
                MenuManager.Instance.Open_Inventory_Menu();
                InventoryManager.Instance.Open_Inventory(inventory);
            }
            else if(MenuManager.Instance.menu_inventory.activeSelf)
            {
                InventoryManager.Instance.Close_Inventory();
                MenuManager.Instance.Close_Menu();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!MenuManager.isPaused)
            {
                Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, interact_range, obj_mask);
                GameObject interact_obj = null;
                float min_dist = Mathf.Infinity;
                foreach (Collider2D neighbor in neighbors)
                {
                    if (neighbor.gameObject.tag == "Flora" && neighbor.GetComponentInParent<PlantEntity>() != null)
                    {
                        float dist = Vector2.Distance(transform.position, neighbor.transform.position);
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            interact_obj = neighbor.gameObject;
                        }
                    }
                }
                if (interact_obj != null)
                {
                    Debug.Log("Interacting with " + interact_obj.name);
                    MenuManager.Instance.Open_Harvest_Menu(interact_obj.GetComponentInParent<PlantEntity>());
                }
            }
            else if (MenuManager.Instance.menu_harvest.activeSelf)
            {
                MenuManager.Instance.plant_manager.Close();
                MenuManager.Instance.Close_Menu();
            }
        }

        //non paused
        if (MenuManager.isPaused || !can_move)
        {
            input_dir = Vector2.zero;
            roll_dir = Vector2.zero;
            roll_speed = 0;
            knock_speed = 0;
            roll_t = roll_cd;
            return;
        }

        //wasd
        input_dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //input_dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input_dir = Circular_Input(input_dir);

        //space
        if (ib_dodge_t > 0) ib_dodge_t -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Space)) ib_dodge_t = input_buffer_dodge;
        //m1
        if (ib_attack_0_t > 0) ib_attack_0_t -= Time.deltaTime;
        if (Input.GetMouseButton(0)) ib_attack_0_t = input_buffer_attack;
        //m2
        if (ib_attack_1_t > 0) ib_attack_1_t -= Time.deltaTime;
        if (Input.GetMouseButton(1)) ib_attack_1_t = input_buffer_attack;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Level Trigger" && GetComponent<CapsuleCollider2D>().IsTouching(collision))
        {
            if (collision.GetComponent<SceneTransition>() != null)
            {
                collision.GetComponent<SceneTransition>().Transition_Scenes();
                state = PlayerState.Normal;
                Destroy(collision.gameObject);
            }
        }
        else
        {
            ItemWorld item_world = collision.GetComponentInParent<ItemWorld>();
            if (item_world != null && !item_world.picked_up)
            {
                //only hit the capsule collider
                if (GetComponent<CapsuleCollider2D>().IsTouching(collision))
                {
                    Debug.Log("Touched " + collision.gameObject);
                    int remainder = inventory.AddItem(item_world.GetItem());
                    if (remainder <= 0)
                    {
                        item_world.picked_up = true;
                        item_world.DestroySelf();
                    }
                    else
                    {
                        item_world.SetAmount(remainder);
                    }
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Flora" && collision.GetComponent<PlantEntity>() != null)
        {
            if (!interactable_objs.Contains(collision.gameObject))
                interactable_objs.Add(collision.gameObject);

            if (interactable_objs.Count > 0 && !key_prompt_anim.gameObject.activeSelf)
            {
                key_prompt_anim.gameObject.SetActive(true);
                key_prompt_anim.SetTrigger("fade_in");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Flora" && collision.GetComponent<PlantEntity>() != null)
        {
            if(interactable_objs.Contains(collision.gameObject))
                interactable_objs.Remove(collision.gameObject);

            if (interactable_objs.Count <= 0 && key_prompt_anim.GetCurrentAnimatorStateInfo(0).IsName("key_fade_in"))
            {
                key_prompt_anim.SetTrigger("fade_out");
                StartCoroutine(Prompt_Wait());
            }
        }
    }

    public IEnumerator Prompt_Wait()
    {
        yield return new WaitForSeconds(1);
        key_prompt_anim.gameObject.SetActive(false);
    }

    // Update is called once per frame
    protected override void Update()
    {
        Handle_Inputs();
        if (Time.timeScale == 0) return;

        switch (state)
        {
            case PlayerState.Normal:

                if (input_dir != Vector2.zero) last_input_dir = input_dir;

                //animation stuff
                if (input_dir != Vector2.zero)
                {
                    run_t = 0.1f;
                    char_anim.SetFloat("idle_t", 0);
                }
                else
                {
                    if (run_t > 0) run_t -= Time.deltaTime;
                    char_anim.SetFloat("idle_t", char_anim.GetFloat("idle_t") + Time.deltaTime);
                }

                if (isAnimInterruptable())
                {
                    char_anim.SetBool("isRunning", run_t > 0);
                    if (Mathf.Abs(last_input_dir.x) > 0.05f) char_t.localScale = new Vector3(Mathf.Sign(last_input_dir.x), 1f, 1f);
                }
                else
                {
                    if (run_t > 0) char_anim.SetTrigger("runTrigger");
                    
                }

                if (ib_dodge_t > 0 && roll_t <= 0)
                {
                    roll_dir = last_input_dir;
                    roll_speed = max_roll_speed;
                    state = PlayerState.Rolling;
                    if (roll_dir.x != 0) char_t.localScale = new Vector3(Mathf.Sign(roll_dir.x), 1f, 1f);
                    char_anim.SetFloat("idle_t", 0);
                }
                if (ib_attack_0_t > 0 && attack_m_t <= 0)
                {
                    char_anim.SetTrigger("thrust");
                    state = PlayerState.Attacking;
                    rb.velocity = Vector2.zero;
                    attack_mouse_pos = GetMouseWorldPos();
                    char_t.localScale = new Vector3(Mathf.Sign((attack_mouse_pos - transform.position).x), 1f, 1f);
                    run_t = 0;
                    char_anim.SetFloat("idle_t", 0);
                }
                if (ib_attack_1_t > 0 && attack_r_t <= 0)
                {
                    char_anim.SetTrigger("throw");
                    state = PlayerState.Attacking;
                    rb.velocity = Vector2.zero;
                    attack_mouse_pos = GetMouseWorldPos();
                    char_t.localScale = new Vector3(Mathf.Sign((attack_mouse_pos - transform.position).x), 1f, 1f);
                    run_t = 0;
                    char_anim.SetFloat("idle_t", 0);
                }
                break;
            case PlayerState.Rolling:
                if (roll_dir.x != 0) char_t.localScale = new Vector3(Mathf.Sign(roll_dir.x), 1f, 1f);
                roll_speed -= roll_speed * roll_speed_mod * Time.deltaTime;
                if (roll_speed < roll_speed_min)
                {
                    roll_t = roll_cd;
                    state = PlayerState.Normal;
                }
                break;
            case PlayerState.Attacking:
                break;
        }
        Update_Knock();

        if (attack_r_t > 0)
        {
            attack_r_t -= Time.deltaTime;
        }
        if (attack_m_t > 0)
        {
            attack_m_t -= Time.deltaTime;
        }
        if (roll_t > 0)
        {
            roll_t -= Time.deltaTime;
        }
    }

    public void Do_Attack_M()
    {
        char_t.localScale = new Vector3(Mathf.Sign((attack_mouse_pos - transform.position).x), 1f, 1f);

        Vector2 click_pos = attack_mouse_pos - attack_r_origin.position;
        Vector2 click_dir = click_pos.normalized;
        last_input_dir = click_dir;

        float angle = Mathf.Atan2(click_dir.y, click_dir.x) * Mathf.Rad2Deg;
        Instantiate(attack_m_effect, (Vector2)attack_r_origin.position, Quaternion.Euler(0, 0, angle + 90));

        //hitbox center, angle, size
        RaycastHit2D hit = Physics2D.Raycast(transform.position, click_dir, attack_m_range, obj_mask);
        float range = hit.collider != null ? hit.distance : attack_m_range;
        Vector2 hb_size = new Vector2(range, attack_m_width);
        Vector2 hb_center = (Vector2)transform.position + (click_dir * range / 2);
        float hb_angle = Mathf.Atan2(click_dir.y, click_dir.x) * Mathf.Rad2Deg;

        List<GameObject> hit_objs = new List<GameObject>();
        Collider2D[] cols = Physics2D.OverlapBoxAll(hb_center, hb_size, hb_angle, entity_mask);
        foreach (Collider2D col in cols)
        {
            if (col.gameObject != gameObject && !hit_objs.Contains(col.gameObject))
            {
                hit_objs.Add(col.gameObject);
                Debug.Log(col.transform.root.gameObject.name + " was hit by melee");
                Entity hit_entity = col.GetComponentInParent<Entity>();
                if (hit_entity != null)
                {
                    hit_entity.Take_Damage(1, this);
                }
            }
        }
        last_input_dir = click_dir;
    }

    public void End_Attack_M()
    {
        state = PlayerState.Normal;
        attack_m_t = attack_m_cd;
    }

    public void Do_Attack_R()
    {
        char_t.localScale = new Vector3(Mathf.Sign((attack_mouse_pos - transform.position).x), 1f, 1f);

        Vector2 click_pos = attack_mouse_pos - attack_r_origin.position;
        Vector2 click_dir = click_pos.normalized;
        last_input_dir = click_dir;

        float proj_angle = Mathf.Atan2(click_dir.y, click_dir.x) * Mathf.Rad2Deg;

        Collider2D[] cols = Physics2D.OverlapCircleAll(attack_r_origin.position, 0.1f, obj_mask);
        if(cols.Length > 0)
        {
            Debug.Log("OVERLAP SPAWN WITH WALL");
            GameObject proj_obj = Instantiate(attack_r_prefab, transform.position, Quaternion.Euler(new Vector3(0, 0, proj_angle)));
            //proj_obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, proj_angle));
            Projectile proj = proj_obj.GetComponent<Projectile>();
            proj.Init_Projectile(attack_r_speed, attack_r_range, gameObject);
        }
        else
        {
            GameObject proj_obj = Instantiate(attack_r_prefab, attack_r_origin.position, Quaternion.Euler(new Vector3(0, 0, proj_angle)));
            //proj_obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, proj_angle));
            Projectile proj = proj_obj.GetComponent<Projectile>();
            proj.Init_Projectile(attack_r_speed, attack_r_range, gameObject);
        }
    }

    public void End_Attack_R()
    {
        state = PlayerState.Normal;
        attack_r_t = attack_r_cd;
    }

    public override void Take_Damage(int amount, Entity source)
    {
        if(state != PlayerState.Rolling)
        {
            Debug.Log("Ow I took Damage");
            knock_dir = (transform.position - source.transform.position).normalized;
            knock_speed = max_knock_speed;

            Hit_Effect(knock_dir);
        }
    }

    public void Attract_Items()
    {
        Collider2D[] nearby_items = Physics2D.OverlapCircleAll(transform.position, item_magnetic_range, item_mask);
        foreach (Collider2D item in nearby_items)
        {
            float r = Vector2.Distance(transform.position, item.transform.position);
            float r2 = Mathf.Pow((item_magnetic_range - r) / item_magnetic_range, 1.25f);
            float f = item_magnetic_force * r2;
            Vector2 dir = transform.position - item.transform.position;
            item.GetComponent<Rigidbody2D>().AddForce(dir.normalized * f * Time.deltaTime); 
        }
    }

    protected override void FixedUpdate()
    {
        //Attract_Items();

        switch (state)
        {
            case PlayerState.Normal:
                rb.velocity = input_dir * move_speed + knock_dir * knock_speed;
                break;
            case PlayerState.Rolling:
                rb.velocity = roll_dir * roll_speed;
                break;
            case PlayerState.Attacking:
                rb.velocity = knock_dir * knock_speed;
                break;
        }
    }
}
