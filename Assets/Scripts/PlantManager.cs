using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantManager : MonoBehaviour
{
    public List<PlantPart> plant_parts = new List<PlantPart>();
    public Image img_icon;
    public PlantPart selected_part;
    public Button harvest_button;
    public Text harvest_description;

    private PlantEntity plant_entity;
    private GameObject plant_pref;

    // Start is called before the first frame update

    public void Init(PlantEntity plant)
    {
        if (plant == null) return;
        plant_entity = plant;
        GameObject plant_obj = Instantiate(plant_entity.ui_plant_pref, transform);
        plant_obj.transform.SetParent(transform);

        plant_pref = plant_obj;

        plant_parts = new List<PlantPart>();
        foreach (PlantPart part in GetComponentsInChildren<PlantPart>())
        {
            plant_parts.Add(part);
            part.manager = this;
            part.Deselect();
        }
        OnPartDeselected(null);
        //StartCoroutine(PreserveRatio());
    }

    public IEnumerator PreserveRatio()
    {
        yield return new WaitForEndOfFrame();
        float h_offset = plant_pref.GetComponent<RectTransform>().rect.height - plant_pref.GetComponent<RectTransform>().rect.width;
        float width = plant_pref.GetComponent<RectTransform>().sizeDelta.x * plant_pref.GetComponent<RectTransform>().localScale.x;
        float height = plant_pref.GetComponent<RectTransform>().sizeDelta.x * plant_pref.GetComponent<RectTransform>().localScale.y;
        Debug.Log(width + " : " + height + " : " + h_offset);
        plant_pref.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height - h_offset);
    }

    private void OnDisable()
    {
        plant_parts = new List<PlantPart>();
        plant_entity = null;
        Destroy(plant_pref);
    }


    public void OnPartDeselected(PlantPart sel)
    {
        img_icon.sprite = null;
        img_icon.color = new Color(1f, 1f, 1f, 0f);
        selected_part = null;
        harvest_button.interactable = false;
        harvest_description.text = "";
    }

    public void OnPartSelected(PlantPart sel)
    {
        img_icon.sprite = sel.spr_icon;
        img_icon.color = new Color(1f, 1f, 1f, 1f);
        selected_part = sel;
        harvest_button.interactable = true;
        harvest_description.text = sel.part_description;

        foreach (PlantPart part in plant_parts)
        {
            if (part != sel) part.Deselect();
        }
    }

    public void Close()
    {
        OnPartDeselected(null);
    }

    public void Harvest()
    {
        if (selected_part != null)
        {
            Debug.Log("Harvested " + selected_part.part_description);
            if (plant_entity != null)
            {
                plant_entity.Harvest();
                if (selected_part.item_drop != null)
                {
                    ItemWorld.SpawnItemWorld(plant_entity.transform.position, selected_part.item_drop);
                }
            }
            OnPartDeselected(null);
            GetComponentInParent<MenuManager>().Close_Menu();
        }
    }
}
