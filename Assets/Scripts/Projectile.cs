using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    public float proj_speed;
    public float proj_range;

    private GameObject parent_obj;
    private float proj_life;
    private bool initialized = false;

    public GameObject hit_pref;
    public GameObject hit_entity_pref;
    public Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init_Projectile(float speed, float range, GameObject parent)
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();

        proj_speed = speed;
        proj_range = range;
        if(speed > 0 ) proj_life = range / speed;

        parent_obj = parent;

        initialized = true;
    }

    private void Death_Fade()
    {
        proj_life = 0;
        anim.SetTrigger("fade");
        Debug.Log("Fade");
        Destroy(gameObject, 0.2f);
    }

    private void Death_Enemy(Transform hit_parent)
    {
        proj_life = 0;
        GameObject hit = Instantiate(hit_entity_pref, hit_parent.position, transform.rotation);
        hit.transform.SetParent(hit_parent);
        hit.transform.localPosition = transform.right * -0.15f;
        Destroy(gameObject);
    }

    private void Death_Wall()
    {
        proj_life = 0;
        StartCoroutine(wall_delay());
    }

    private IEnumerator wall_delay()
    {
        yield return new WaitForSeconds(0.1f / proj_speed);
        GameObject hit = Instantiate(hit_pref, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            proj_life -= Time.deltaTime;
            if(proj_life < 0)
            {
                Death_Fade();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root.gameObject != parent_obj && proj_life > 0)
        {
            Debug.Log(collision.transform.root.gameObject.name + " hit by projectile");
            //hit obstacle
            if(collision.gameObject.layer == 9)
            {
                Death_Wall();
            }
            //hit entity
            else if (collision.gameObject.layer == 8)
            {
                Entity hit_entity = collision.GetComponentInParent<Entity>();
                if (hit_entity != null)
                {
                    hit_entity.Take_Damage(1, parent_obj.GetComponent<Entity>());
                    Death_Enemy(hit_entity.spear_embed_parent);
                }
                else
                {
                    Death_Fade();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.right * proj_speed;
    }
}
