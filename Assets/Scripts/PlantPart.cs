using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlantPart : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    private Image img;
    public bool selected = false;
    public Sprite spr_unselected;
    public Sprite spr_selected;
    public Sprite spr_clicked;
    public Sprite spr_icon;
    [TextArea]
    public string part_description;

    public PlantManager manager;

    public Item item_drop;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click");
        if(!selected)
        {
            img.sprite = spr_clicked;
            selected = true;
            manager.OnPartSelected(this);
        }
        else
        {
            img.sprite = spr_selected;
            selected = false;
            manager.OnPartDeselected(this);
        }
    }

    public void Deselect()
    {
        selected = false;
        img = GetComponent<Image>();
        img.alphaHitTestMinimumThreshold = 1f;
        img.sprite = spr_unselected;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Up");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Enter");
        if(!selected) img.sprite = spr_selected;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exit");
        if (!selected) img.sprite = spr_unselected;
    }
}
