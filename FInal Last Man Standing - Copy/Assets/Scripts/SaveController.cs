﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ZombiePrefabEntry
{
    public string prefabName;
    public GameObject prefab;
}

public class SaveController : MonoBehaviour
{
    public static SaveController instance;
    public GameObject zombiePrefab;
    public List<ZombiePrefabEntry> zombiePrefabs; // ลาก prefab เข้า Inspector

    private string saveLocation;
    private InventoryController inventoryController;
    void Start()
    {
        if (instance == null)
        {
            instance = this; // สร้าง Instance ถ้ายังไม่มี
            DontDestroyOnLoad(gameObject); // ให้ SaveController ไม่ถูกทำลายเมื่อโหลดฉากใหม่
        }
        else
        {
            Destroy(gameObject); // ถ้ามี Instance แล้วให้ทำลายตัวนี้
        }

        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();

        
    }
    public void SaveGame()
    {
        List<SceneObjectSaveData> sceneObjects = new List<SceneObjectSaveData>();
        GameObject[] allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allSceneObjects)
        {
            if (obj.scene.IsValid() && obj.scene.name == "2-1 Room" && obj.CompareTag("item"))
            {
                sceneObjects.Add(new SceneObjectSaveData
                {
                    objectName = obj.name,
                    isActive = obj.activeSelf
                });
            }
        }

        List<ZombieSaveData> zombiesData = new List<ZombieSaveData>();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject zombie in allObjects)
        {
            if ((zombie.CompareTag("Enemy") || zombie.CompareTag("MiniBoss") || zombie.CompareTag("Boss")) &&
                zombie.scene.IsValid() && zombie.scene.name == "2-1 Room")
            {
                zombiesData.Add(new ZombieSaveData
                {
                    position = zombie.transform.position,
                    zombieName = zombie.name.Replace("(Clone)", "").Trim(),
                    isActive = zombie.activeSelf
                });
            }
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        Unit playerUnit = playerObject.GetComponent<Unit>();
        if (playerUnit == null) return;

        SaveData saveData = new SaveData
        {
            playerPosition = playerObject.transform.position,
            playerHP = GameManager.instance.savedHP,
            playerEnergy = GameManager.instance.savedEnergy,
            playerGold = GameManager.instance.gold, // ✅ บันทึกจำนวนเงิน
            zombiesData = zombiesData,
            inventorySaveData = inventoryController.GetInventoryItems(),
            sceneObjectData = sceneObjects
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Game Saved Successfully!");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation)) return;

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;
            player.GetComponent<Unit>().currentHP = saveData.playerHP;
            player.GetComponent<Unit>().currentEnergy = saveData.playerEnergy;
            GameManager.instance.savedHP = saveData.playerHP;
            GameManager.instance.savedEnergy = saveData.playerEnergy;
            GameManager.instance.gold = saveData.playerGold; // ✅ โหลดเงิน
            GameManager.instance.UpdateGoldUI(); // ✅ อัปเดต UI ทันทีถ้าอยู่ใน scene ที่มี goldText
        }

        List<GameObject> allZombies = new List<GameObject>();
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("MiniBoss"));
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("Boss"));

        foreach (ZombieSaveData zombieData in saveData.zombiesData)
        {
            foreach (GameObject zombie in allZombies)
            {
                string zombieNameInScene = zombie.name.Replace("(Clone)", "").Trim();
                if (zombieNameInScene == zombieData.zombieName)
                {
                    zombie.transform.position = zombieData.position;
                    zombie.SetActive(zombieData.isActive);
                    break;
                }
            }
        }

        foreach (SceneObjectSaveData objData in saveData.sceneObjectData)
        {
            GameObject[] allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allSceneObjects)
            {
                if (obj.scene.IsValid() && obj.scene.name == "2-1 Room" && obj.name == objData.objectName)
                {
                    obj.SetActive(objData.isActive);
                    InteractObject interactObj = obj.GetComponent<InteractObject>();
                    if (interactObj != null && interactObj.interact2 != null)
                        interactObj.interact2.SetActive(objData.isActive);
                    break;
                }
            }
        }

        inventoryController.SetInventoryItems(saveData.inventorySaveData);
    }
}
