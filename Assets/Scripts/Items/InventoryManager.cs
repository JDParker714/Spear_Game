using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance { get { return _instance; } }

    public static int inventory_size = 16;

    public Transform item_slot_template;
    public Transform item_slot_parent;

    public Transform item_icon_template;
    public Transform item_icon_parent;

    private List<ItemSlot> inv_slots = new List<ItemSlot>();

    private Inventory ref_inventory;

    private DragDrop picked_up_item;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        Init_Inventory();
    }

    public void PickupItem(DragDrop pickup)
    {
        picked_up_item = pickup;
    }

    public void DropItem()
    {
        picked_up_item = null;
    }

    public DragDrop GetHeldItem()
    {
        return picked_up_item;
    }

    public bool isHoldingItem()
    {
        return picked_up_item != null;
    }

    public DragDrop CreateNewItem(Item item)
    {
        if (item == null) return null;

        DragDrop icon = Instantiate(item_icon_template, item_icon_parent).GetComponent<DragDrop>();
        icon.gameObject.SetActive(true);

        icon.ui_item = new Item { amount = item.amount, itemType = item.itemType };

        icon.GetComponent<Image>().sprite = item.GetSprite();
        if (item.amount > 1)
            icon.GetComponentInChildren<TextMeshProUGUI>().SetText(item.amount.ToString());
        else
            icon.GetComponentInChildren<TextMeshProUGUI>().SetText("");

        return icon;
    }

    private void Update()
    {
        if (picked_up_item != null)
        {
            picked_up_item.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    ItemWorld.SpawnItemWorld(ref_inventory.Get_Pos() + Random.insideUnitCircle.normalized * 1.5f, picked_up_item.ui_item);
                    Destroy(picked_up_item.gameObject);
                    picked_up_item = null;
                }
            }
        }
    }

    public void Init_Inventory()
    {
        //clear
        inv_slots.Clear();
        foreach(Transform child in item_slot_parent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in item_icon_parent)
        {
            Destroy(child.gameObject);
        }
        //create slots
        for (int i = 0; i < inventory_size; i++)
        {
            Transform slot = Instantiate(item_slot_template, item_slot_parent);
            slot.gameObject.SetActive(true);
        }
        //add to list
        foreach (Transform child in item_slot_parent)
        {
            if (child.GetComponent<ItemSlot>() != null)
            {
                inv_slots.Add(child.GetComponent<ItemSlot>());
            }
        }
    }

    public void Open_Inventory(Inventory _inv)
    {
        ref_inventory = _inv;

        //init

        //destroy icons
        foreach(Transform child in item_icon_parent)
        {
            Destroy(child.gameObject);
        }
        //reset slots
        foreach(ItemSlot slot in inv_slots)
        {
            slot.slotted_item = null;
        }

        StartCoroutine(wait_Open_Inventory());
    }

    private IEnumerator wait_Open_Inventory()
    {
        yield return new WaitForEndOfFrame();

        if (ref_inventory != null)
        {
            //go from inventory -> slotted icons
            for (int i = 0; i < ref_inventory.GetItemList().Count; i++)
            {
                //make sure it corresponds to an inventory slot
                if (i >= inv_slots.Count) break;

                Item item = ref_inventory.GetItemList()[i];

                ItemSlot slot = inv_slots[i];

                if (item != null)
                {
                    DragDrop icon = Instantiate(item_icon_template, item_icon_parent).GetComponent<DragDrop>();
                    icon.gameObject.SetActive(true);

                    icon.ui_item = new Item { amount = item.amount, itemType = item.itemType };
                    icon.prev_slot = slot;
                    slot.slotted_item = icon;
                    icon.GetComponent<RectTransform>().anchoredPosition = slot.GetComponent<RectTransform>().anchoredPosition;
                    icon.GetComponent<Image>().sprite = item.GetSprite();
                    if (item.amount > 1)
                        icon.GetComponentInChildren<TextMeshProUGUI>().SetText(item.amount.ToString());
                    else
                        icon.GetComponentInChildren<TextMeshProUGUI>().SetText("");
                }
            }
        }
    }

    public void Close_Inventory()
    {
        //go from slotted icons to inventory
        if (ref_inventory == null) return;

        List<Item> new_inventory_list = new List<Item>();
        //iterate through inventory slots
        for (int i = 0; i < inv_slots.Count; i++)
        {
            //make sure it corresponds to an item slot
            if (i >= ref_inventory.GetItemList().Count) break;

            ItemSlot slot = inv_slots[i];
            if(slot.slotted_item != null)
            {
                if (slot.slotted_item.ui_item != null)
                {
                    new_inventory_list.Add(slot.slotted_item.ui_item);
                    continue;
                }
            }
            new_inventory_list.Add(null);
        }
        ref_inventory.SetItemList(new_inventory_list);

        if(picked_up_item != null)
        {
            ItemWorld.SpawnItemWorld(ref_inventory.Get_Pos() + Random.insideUnitCircle * 1.5f, picked_up_item.ui_item);
        }
        picked_up_item = null;

        //destroy icons
        foreach (Transform child in item_icon_parent)
        {
            Destroy(child.gameObject);
        }

        //reset slots
        foreach (ItemSlot slot in inv_slots)
        {
            slot.slotted_item = null;
        }
    }
}
