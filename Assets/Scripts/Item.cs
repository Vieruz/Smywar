using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData data;
    public float dropRate;
    public Sprite image;

    public void Take()
    {
        // Animation de prendre
        Destroy(gameObject);
    }

    public void Use(PlayerController p)
    {
        switch (data.type)
        {
            case 0:
                DrinkPotion(p);
                break;
            case 1:
                DrinkMana(p);
                break;
            default:
                Debug.Log("Unknown potion: " + gameObject.name);
                break;
        }
    }

    public void DrinkPotion(PlayerController p)
    {
        // Boire une potion de soin
        p.HealLP(p.PlayerData.MaxLP);
    }

    public void DrinkMana(PlayerController p)
    {
        // Boire une potion de mana
        p.DrinkMP(p.PlayerData.MaxMP);
    }
}

[Serializable]
public struct ItemData
{
    public int type;
    public int quantity;
}