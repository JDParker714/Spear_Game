using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static MenuManager _instance;
    public static MenuManager Instance { get { return _instance; } }

    public static bool isPaused;
    //plant manager
    public GameObject menu_harvest;
    public GameObject menu_inventory;
    public PlantManager plant_manager;

    //ew
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
    }

    //ew
    private void Start()
    {
        Close_Menu();
    }

    //ew
    public void Open_Harvest_Menu(PlantEntity plant)
    {
        menu_harvest.SetActive(true);
        plant_manager.Init(plant);
        Time.timeScale = 0f;
        isPaused = true;
    }

    //ew
    public void Open_Inventory_Menu()
    {
        menu_inventory.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
    
    //ew
    public void Close_Menu()
    {
        isPaused = false;
        menu_harvest.SetActive(false);
        menu_inventory.SetActive(false);
        Time.timeScale = 1f;
    }
}
