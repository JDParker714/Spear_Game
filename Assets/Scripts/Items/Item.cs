using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public enum ItemType
    {
        FruitBlueberry,
        RootBlueberry,
        LeafBlueberry,
        Coin
    }

    public ItemType itemType;
    public int amount;

    public static int stack_limit = 69;

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            case ItemType.FruitBlueberry: return ItemAssets.Instance.FruitBlueberry_spr;
            case ItemType.RootBlueberry: return ItemAssets.Instance.RootBlueberry_spr;
            case ItemType.LeafBlueberry: return ItemAssets.Instance.LeafBlueberry_spr;
            case ItemType.Coin: return ItemAssets.Instance.Coin_spr;
            default: return ItemAssets.Instance.Coin_spr;
        }
    }

    public bool IsStackable()
    {
        switch (itemType)
        {
            case ItemType.FruitBlueberry: return true;
            case ItemType.RootBlueberry: return true;
            case ItemType.LeafBlueberry: return true;
            case ItemType.Coin: return true;
            default: return true;
        }
    }
}
