using System;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public string Uid;
    public string Name;
    public PlayerClass PlayerClass;

    public int Level;
    public int Exp;
    public int LevelPoints;
    public int Floor;

    public int Strength;
    public int Dexterity;
    public int Stamina;
    public int Intelligence;
    public int Wisdom;

    public int LP;
    public int MaxLP;
    public int MP;
    public int MaxMP;
    public int Gold;
    public bool Stun;
    public bool Heal;

    public Vector3 playerPosition;
    public Vector3 targetPosition;
    public string spellToUid;

    public ItemData[] Inventory;
}

public enum PlayerClass
{
    Knight,
    Mage,
    Priest
}
