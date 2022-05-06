using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour, IPointerDownHandler
{
    public DragDrop slotted_item;

    public void OnPointerDown(PointerEventData eventData)
    {
        //if a drag and drop item is dropped on it
        Debug.Log("Point Down Slot");
        if(InventoryManager.Instance.isHoldingItem() && eventData.button == PointerEventData.InputButton.Left)
        {
            DragDrop held_item = InventoryManager.Instance.GetHeldItem();
            //and there is no inventory item slotted
            if (slotted_item == null)
            {
                Debug.Log("Drop");
                held_item.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                if(held_item.prev_slot != null)
                    held_item.prev_slot.slotted_item = null;
                held_item.prev_slot = this;
                slotted_item = held_item;
                InventoryManager.Instance.DropItem();
                held_item.Drop();
            }
        }
        else if (InventoryManager.Instance.isHoldingItem() && eventData.button == PointerEventData.InputButton.Right)
        {
            DragDrop held_item = InventoryManager.Instance.GetHeldItem();
            Item item = held_item.ui_item;
            //and there is no inventory item slotted
            if (slotted_item == null)
            {
                Debug.Log("Drop 1");
                if(item.amount == 1)
                {
                    held_item.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                    if (held_item.prev_slot != null)
                        held_item.prev_slot.slotted_item = null;
                    held_item.prev_slot = this;
                    slotted_item = held_item;
                    InventoryManager.Instance.DropItem();
                    held_item.Drop();
                }
                else
                {
                    item.amount--;
                    if (item.amount > 1)
                        held_item.GetComponentInChildren<TextMeshProUGUI>().SetText(item.amount.ToString());
                    else
                        held_item.GetComponentInChildren<TextMeshProUGUI>().SetText("");

                    Item other_one_item = new Item { amount = 1, itemType = item.itemType };

                    DragDrop new_item = InventoryManager.Instance.CreateNewItem(other_one_item);

                    new_item.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                    if (new_item.prev_slot != null)
                        new_item.prev_slot.slotted_item = null;
                    new_item.prev_slot = this;
                    slotted_item = new_item;

                    held_item.transform.SetAsLastSibling();
                }
            }
        }
    }
}
