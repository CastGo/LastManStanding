using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZombieSaveData
{
    public Vector3 position;  // ตำแหน่งของ Zombie
    public string zombieName; // ชื่อของ Zombie
    public bool isActive;
}

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition; // ตำแหน่งของ Player
    public int playerHP; // ค่าพลังชีวิตของ Player
    public List<ZombieSaveData> zombiesData; // รายการข้อมูลของ zombie
}
