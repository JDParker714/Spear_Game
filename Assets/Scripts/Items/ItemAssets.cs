using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    private static ItemAssets _instance;
    public static ItemAssets Instance { get { return _instance; } }

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

    public GameObject item_world_pref;

    public Sprite FruitBlueberry_spr;
    public Sprite RootBlueberry_spr;
    public Sprite LeafBlueberry_spr;
    public Sprite Coin_spr;

    public GameObject hit_particle_pref;
}
