using System;
using UnityEngine;

[Serializable]
public struct MonsterData
{
    public string Uid;
    public string Name;
    public int monsterType;
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

    public Vector3 monsterPosition;
}