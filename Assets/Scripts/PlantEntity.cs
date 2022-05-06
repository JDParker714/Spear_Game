using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEntity : MonoBehaviour
{
    public GameObject ui_plant_pref;
    public SpriteRenderer rend;

    public Sprite spr_normal;
    public Sprite spr_highlighted;

    private void Awake()
    {
        GetComponent<CircleCollider2D>().radius = PlayerController.interact_range;
        rend.sprite = spr_normal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null && !collision.isTrigger)
        {
            rend.sprite = spr_highlighted;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null && !collision.isTrigger)
        {
            rend.sprite = spr_normal;
        }
    }

    public void Harvest()
    {
        Destroy(gameObject);
    }
}
