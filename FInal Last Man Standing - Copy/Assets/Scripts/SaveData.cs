using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneObjectSaveData
{
    public string objectName;      // ใช้ name ของ GameObject เพื่อระบุตัวตน
    public bool isActive;          // บันทึกว่า active ไหม
    public string objectTag;
    public Vector3 position;
}

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
    public int playerEnergy;
    public int playerGold;
    public List<ZombieSaveData> zombiesData; // รายการข้อมูลของ zombie
    public List<InventorySaveData> inventorySaveData;
    public List<SceneObjectSaveData> sceneObjectData;
}
