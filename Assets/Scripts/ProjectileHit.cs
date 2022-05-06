using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHit : MonoBehaviour
{
    public Animator anim;
    public float fade_time;
    public LayerMask obj_mask;

    // Start is called before the first frame update
    void Start()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, 0.5f, obj_mask);
        if(hit.collider!=null)
        {
            transform.position = hit.point;
        }
        StartCoroutine(Fade_Death());   
    }

    private IEnumerator Fade_Death()
    {
        yield return new WaitForSeconds(fade_time);
        anim.SetTrigger("fade");
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
