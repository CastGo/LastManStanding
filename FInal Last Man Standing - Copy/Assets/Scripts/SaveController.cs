using Cinemachine;
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
    }


    public void SaveGame()
    {
        List<ZombieSaveData> zombiesData = new List<ZombieSaveData>();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject zombie in allObjects)
        {
            // ✅ เช็คว่าเป็น Enemy, อยู่ใน scene จริง (ไม่ใช่ prefab), และ scene ชื่อ 2-1 Room
            if ((zombie.CompareTag("Enemy") || zombie.CompareTag("MiniBoss") || zombie.CompareTag("Boss")) && zombie.scene.IsValid() &&zombie.scene.name == "2-1 Room")
            {
                ZombieSaveData zombieData = new ZombieSaveData
                {
                    position = zombie.transform.position,
                    zombieName = zombie.name.Replace("(Clone)", "").Trim(),
                    isActive = zombie.activeSelf
                };
                zombiesData.Add(zombieData);
            }
        }

        // หาตัว Player และดึงค่า currentHP
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("SaveGame Error: Player object not found!");
            return; // ออกจากฟังก์ชันถ้าไม่มี Player
        }

        Unit playerUnit = playerObject.GetComponent<Unit>();
        if (playerUnit == null)
        {
            Debug.LogError("SaveGame Error: Player does not have a Unit component!");
            return; // ออกจากฟังก์ชันถ้า Player ไม่มี Unit
        }

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            playerHP = GameManager.instance.savedHP,
            playerEnergy = GameManager.instance.savedEnergy,
            zombiesData = zombiesData
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData)); // เขียนข้อมูลลงไฟล์
        Debug.Log("Game Saved Successfully!");
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            // โหลดตำแหน่งและ HP ของ Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
                player.GetComponent<Unit>().currentHP = saveData.playerHP;
                GameManager.instance.savedHP = saveData.playerHP;
                player.GetComponent<Unit>().currentEnergy = saveData.playerEnergy;
                GameManager.instance.savedEnergy = saveData.playerEnergy;
            }

            List<GameObject> allZombies = new List<GameObject>();
            allZombies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
            allZombies.AddRange(GameObject.FindGameObjectsWithTag("MiniBoss"));
            allZombies.AddRange(GameObject.FindGameObjectsWithTag("Boss"));
            // 🔁 อัปเดต zombie ทั้งหมดตาม saveData
            foreach (ZombieSaveData zombieData in saveData.zombiesData)
            {
                foreach (GameObject zombie in allZombies)
                {
                    // เปรียบเทียบชื่อ zombie แบบ Trim แล้วเอา (Clone) ออก
                    string zombieNameInScene = zombie.name.Replace("(Clone)", "").Trim();
                    if (zombieNameInScene == zombieData.zombieName)
                    {
                        zombie.transform.position = zombieData.position;
                        zombie.SetActive(zombieData.isActive);
                        break;
                    }
                }
            }

            GameManager.instance.isLoadingFromSave = false;
        }
    }
}
