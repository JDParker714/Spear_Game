using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Entity : MonoBehaviour
{
    //entity - base thing that needs to react to global events like damage
    [Header("Base Entity Stuff")]
    public int max_health;
    protected int health;
    public Transform spear_embed_parent;
    public GameObject blood_effect;
    //knockback
    [Header("Knockback Stuff")]
    protected Vector2 knock_dir;
    public static float max_knock_speed = 8f;
    public static float knock_speed_mod = 4f;
    public static float min_knock_speed = 4f;
    protected float knock_speed = 5f;

    public string entity_type = "Entity";
    public string entity_team = "Beasts";
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        health = max_health;
        rb = GetComponent<Rigidbody2D>();
        knock_speed = 0f;
        if (spear_embed_parent == null) spear_embed_parent = transform;
        if (blood_effect != null) blood_effect.SetActive(false);
    }

    public virtual void Take_Damage(int amount, Entity source)
    {
        //knock
        knock_dir = (transform.position - source.transform.position).normalized;
        knock_speed = max_knock_speed;

        Hit_Effect(knock_dir);
        health -= amount;
        if (blood_effect != null) blood_effect.SetActive(health <= max_health/2);
        if (health <= 0)
        {
            Death();
        }
    }

    protected void Hit_Effect(Vector2 dir)
    {
        StartCoroutine(Damage_Flash());
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Instantiate(ItemAssets.Instance.hit_particle_pref, (Vector2)transform.position + dir.normalized*0.4f, Quaternion.Euler(0,0,angle-90));
    }

    protected IEnumerator Damage_Flash()
    {
        float flash_time = 0.2f;
        foreach (SpriteRenderer rend in GetComponentsInChildren<SpriteRenderer>())
        {
            rend.material.SetInt("_isFlashing", 1);
        }
        yield return new WaitForSeconds(flash_time);

        foreach (SpriteRenderer rend in GetComponentsInChildren<SpriteRenderer>())
        {
            rend.material.SetInt("_isFlashing", 0);
        }

        yield return new WaitForSeconds(flash_time/2f);

        foreach (SpriteRenderer rend in GetComponentsInChildren<SpriteRenderer>())
        {
            rend.material.SetInt("_isFlashing", 1);
        }

        yield return new WaitForSeconds(flash_time/2f);

        foreach (SpriteRenderer rend in GetComponentsInChildren<SpriteRenderer>())
        {
            rend.material.SetInt("_isFlashing", 0);
        }
    }

    protected virtual void Update()
    {
        Update_Knock();
    }

    protected void Update_Knock()
    {
        if (knock_speed > 0)
        {
            if (knock_speed < min_knock_speed) knock_speed = 0;
            knock_speed = Mathf.Clamp(knock_speed, 0, knock_speed - (knock_speed * knock_speed_mod * Time.deltaTime));
        }
    }

    protected virtual void FixedUpdate()
    {
        rb.velocity = knock_dir * knock_speed;
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }
}
