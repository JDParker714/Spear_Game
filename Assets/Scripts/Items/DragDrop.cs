using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    private RectTransform rect_t;
    private CanvasGroup canvas_group;
    private Canvas canvas;
    public ItemSlot prev_slot;

    public Item ui_item;

    private void Awake()
    {
        rect_t = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvas_group = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //pointer down
        Debug.Log("Pointer Down");
        //pikcup
        if (!InventoryManager.Instance.isHoldingItem() && eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.PickupItem(this);

            PickUp();
        }
        //split half
        else if (!InventoryManager.Instance.isHoldingItem() && eventData.button == PointerEventData.InputButton.Right)
        {
            int half_1 = Mathf.RoundToInt(Mathf.Clamp(ui_item.amount / 2f, 1, Mathf.Infinity));
            int half_2 = ui_item.amount - half_1;

            //cant split amount 1 - so just pickup
            if(ui_item.amount == 1)
            {
                InventoryManager.Instance.PickupItem(this);

                PickUp();
            }
            else
            {
                ui_item.amount = half_1;
                if (ui_item.amount > 1)
                    GetComponentInChildren<TextMeshProUGUI>().SetText(ui_item.amount.ToString());
                else
                    GetComponentInChildren<TextMeshProUGUI>().SetText("");

                Item other_half_item = new Item { amount = half_2, itemType = ui_item.itemType };

                DragDrop new_item = InventoryManager.Instance.CreateNewItem(other_half_item);
                InventoryManager.Instance.PickupItem(new_item);
                new_item.PickUp();
            }
        }
        else if(InventoryManager.Instance.isHoldingItem())
        {
            //already holding item
            DragDrop item = InventoryManager.Instance.GetHeldItem();

            //stack - check for stack max
            if(ui_item.itemType == item.ui_item.itemType && ui_item.IsStackable())
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    int amt_change = 1;
                    int amt = Mathf.Min(amt_change, Item.stack_limit - ui_item.amount);
                    ui_item.amount+= amt;
                    if (ui_item.amount > 1)
                        GetComponentInChildren<TextMeshProUGUI>().SetText(ui_item.amount.ToString());
                    else
                        GetComponentInChildren<TextMeshProUGUI>().SetText("");

                    item.ui_item.amount-= amt;
                    if (item.ui_item.amount <= 0)
                    {
                        InventoryManager.Instance.DropItem();
                        Destroy(item.gameObject);
                    }
                    else if (item.ui_item.amount > 1)
                        item.GetComponentInChildren<TextMeshProUGUI>().SetText(item.ui_item.amount.ToString());
                    else
                        item.GetComponentInChildren<TextMeshProUGUI>().SetText("");
                }
                else
                {
                    int amt_change = item.ui_item.amount;
                    int amt = Mathf.Min(amt_change, Item.stack_limit - ui_item.amount);
                    ui_item.amount += amt;
                    if (ui_item.amount > 1)
                        GetComponentInChildren<TextMeshProUGUI>().SetText(ui_item.amount.ToString());
                    else
                        GetComponentInChildren<TextMeshProUGUI>().SetText("");

                    item.ui_item.amount -= amt;
                    if (item.ui_item.amount <= 0)
                    {
                        InventoryManager.Instance.DropItem();
                        Destroy(item.gameObject);
                    }
                    else if (item.ui_item.amount > 1)
                        item.GetComponentInChildren<TextMeshProUGUI>().SetText(item.ui_item.amount.ToString());
                    else
                        item.GetComponentInChildren<TextMeshProUGUI>().SetText("");
                }
            }
            //swap sel and slotted
            else
            {
                //switch: slot_a = item being dragged prev slot, slot_b = item dropped on prev slot
                //ItemSlot slot_a = item.prev_slot;
                ItemSlot slot_b = prev_slot;

                //swap slots
                item.prev_slot = slot_b;
                prev_slot = null;

                //update

                //set pos
                item.GetComponent<RectTransform>().anchoredPosition = slot_b.GetComponent<RectTransform>().anchoredPosition;
                //GetComponent<RectTransform>().anchoredPosition = slot_a.GetComponent<RectTransform>().anchoredPosition;

                //update slotted items
                //slot_a.slotted_item = null;
                slot_b.slotted_item = item;

                InventoryManager.Instance.PickupItem(this);
                PickUp();

                item.Drop();
            }
        }
    }

    public void PickUp()
    {
        canvas_group.alpha = 0.6f;
        canvas_group.blocksRaycasts = false;
        transform.SetAsLastSibling();
        if(prev_slot!= null) prev_slot.slotted_item = null;
    }

    public void Drop()
    {
        canvas_group.alpha = 1f;
        canvas_group.blocksRaycasts = true;
        //InventoryManager.Instance.is_dragging = false;

        //set pos
        if (prev_slot != null) GetComponent<RectTransform>().anchoredPosition = prev_slot.GetComponent<RectTransform>().anchoredPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //pointer up
        Debug.Log("Pointer Up");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //drag begin
        /*
        canvas_group.alpha = 0.6f;
        canvas_group.blocksRaycasts = false;
        transform.SetAsLastSibling();

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Debug.Log("Split Stack");
            }
            else
            {
                Debug.Log("Get One");
            }
        }
        */
    }

    public void OnDrag(PointerEventData eventData)
    {
        //drag
        //rect_t.anchoredPosition += eventData.delta / canvas.scaleFactor;
        //InventoryManager.Instance.is_dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        /*
        canvas_group.alpha = 1f;
        canvas_group.blocksRaycasts = true;
        //InventoryManager.Instance.is_dragging = false;

        //set pos
        GetComponent<RectTransform>().anchoredPosition = prev_slot.GetComponent<RectTransform>().anchoredPosition;
        */
    }

    public void OnDrop(PointerEventData eventData)
    {
        /*
        if(eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<DragDrop>() != null)
        {
            //switch: slot_a = item being dragged prev slot, slot_b = item dropped on prev slot
            ItemSlot slot_a = eventData.pointerDrag.GetComponent<DragDrop>().prev_slot;
            ItemSlot slot_b = prev_slot;

            //swap slots
            eventData.pointerDrag.GetComponent<DragDrop>().prev_slot = slot_b;
            prev_slot = slot_a;

            //update

            //set pos
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = slot_b.GetComponent<RectTransform>().anchoredPosition;
            GetComponent<RectTransform>().anchoredPosition = slot_a.GetComponent<RectTransform>().anchoredPosition;

            //update slotted items
            slot_a.slotted_item = this;
            slot_b.slotted_item = eventData.pointerDrag.GetComponent<DragDrop>();
        }
        */
    }
}
