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
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject zombie in zombies)
        {
            ZombieSaveData zombieData = new ZombieSaveData
            {
                position = zombie.transform.position,
                zombieName = zombie.name.Replace("(Clone)", "").Trim(),
                isActive = zombie.activeSelf // ✅ บันทึกสถานะเปิด/ปิด
            };
            zombiesData.Add(zombieData);
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
            playerHP = GameObject.FindGameObjectWithTag("Player").GetComponent<Unit>().currentHP,
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
            }

            // 🔁 อัปเดต zombie ทั้งหมดตาม saveData
            foreach (ZombieSaveData zombieData in saveData.zombiesData)
            {
                GameObject existing = GameObject.Find(zombieData.zombieName + "(Clone)");
                if (existing != null)
                {
                    // ถ้ามีอยู่แล้ว ให้ย้ายตำแหน่งและเปิด/ปิดตามที่บันทึก
                    existing.transform.position = zombieData.position;
                    existing.SetActive(zombieData.isActive);
                }
                else
                {
                    // ถ้ายังไม่มี ให้สร้างใหม่
                    GameObject prefab = zombiePrefabs.Find(z => z.prefabName == zombieData.zombieName)?.prefab;
                    if (prefab != null)
                    {
                        GameObject newZombie = Instantiate(prefab, zombieData.position, Quaternion.identity);
                        newZombie.name = zombieData.zombieName;
                        newZombie.SetActive(zombieData.isActive);
                    }
                }
            }

            GameManager.instance.isLoadingFromSave = false;
        }
    }
}
