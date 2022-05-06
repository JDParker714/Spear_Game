using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private int capacity;
    private List<Item> itemList;
    private Transform t;

    public Inventory(Transform _t)
    {
        t = _t;
        capacity = InventoryManager.inventory_size;
        itemList = new List<Item>();
        for (int i = 0; i < capacity; i++)
        {
            itemList.Add(null);
        }
        Debug.Log("Inventory: " + itemList.Count);
    }

    public int AddItem(Item item)
    {
        if (item == null) return -1;

        int amount_full = item.amount;
        int amount_remaining = item.amount;

        for (int i = 0; i < capacity; i++)
        {
            if (amount_remaining <= 0) break;

            Item inv_item = itemList[i];
            if (inv_item == null)
            {
                int amt = Mathf.Min(amount_remaining, Item.stack_limit);
                itemList[i] = new Item { amount = amt, itemType = item.itemType };
                amount_remaining -= amt;
            }
            else if (inv_item.itemType == item.itemType && item.IsStackable())
            {
                int amt = Mathf.Min(amount_remaining, Item.stack_limit - inv_item.amount);
                inv_item.amount += amt;
                amount_remaining -= amt;
            }
        }

        Debug.Log("Added " + item.itemType + " amt: " + (amount_full - amount_remaining) + " to inventory");

        /*
        if(amount_remaining > 0)
        {
            ItemWorld.SpawnItemWorld( (Vector2)t.position + new Vector2(0f, -1f), new Item { amount = amount_remaining, itemType = item.itemType });
        }
        */
        return amount_remaining;
    }

    public bool Can_Store(Item item)
    {
        bool can_store = false;
        for (int i = 0; i < capacity; i++)
        {
            Item inv_item = itemList[i];
            if (inv_item == null)
            {
                return true;
            }
            else if (inv_item.itemType == item.itemType && item.IsStackable())
            {
                return true;
            }
        }
        return can_store;
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }

    public void SetItemList(List<Item> newList)
    {
        itemList = new List<Item>(newList);
    }

    public Vector2 Get_Pos()
    {
        if (t != null)
            return t.position;
        else
            return Vector2.zero;
    }
}
