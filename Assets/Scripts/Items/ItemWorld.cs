using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemWorld : MonoBehaviour
{
    private Item item;
    public bool picked_up = false;

    public static ItemWorld SpawnItemWorld(Vector2 Position, Item item)
    {
        GameObject item_obj = Instantiate(ItemAssets.Instance.item_world_pref, Position, Quaternion.identity);
        ItemWorld item_world = item_obj.GetComponent<ItemWorld>();
        item_world.SetItem(item);

        return item_world;
    }

    public SpriteRenderer sprite_rend;
    public TextMeshProUGUI text_amt;
    public Animator anim;

    private void Awake()
    {
        anim.SetFloat("offset", Random.Range(0f, 2f));
    }

    public void SetAmount(int amt)
    {
        item.amount = amt;
        if (item.amount > 1)
        {
            text_amt.SetText(item.amount.ToString());
        }
        else
        {
            text_amt.SetText("");
        }
    }

    public void SetItem(Item item)
    {
        this.item = item;
        sprite_rend.sprite = item.GetSprite();
        if(item.amount > 1)
        {
            text_amt.SetText(item.amount.ToString());
        }
        else
        {
            text_amt.SetText("");
        }

    }

    public Item GetItem()
    {
        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
