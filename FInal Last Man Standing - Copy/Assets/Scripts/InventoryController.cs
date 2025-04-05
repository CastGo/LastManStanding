using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;
    private bool isInventoryOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        inventoryPanel.SetActive(false);

        CreateEmptySlots();
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

    public List<InventorySaveData> GetInventoryItem()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach(Transform itemslotTranfrom in slotPanel.transform)
        {
            ItemSlot itemSlot = itemslotTranfrom.GetComponent<ItemSlot>();
            if(itemSlot.currentItem != null)
            {
                Item item = itemSlot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = itemslotTranfrom.GetSiblingIndex() });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        for (int i = 0; i < inventorySaveData.Count; i++)
        {
            var data = inventorySaveData[i];
            if (data.slotIndex < slotPanel.transform.childCount)
            {
                ItemSlot itemslot = slotPanel.transform.GetChild(data.slotIndex).GetComponent<ItemSlot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, itemslot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    itemslot.currentItem = item;
                }
            }
        }
    }
    private void CreateEmptySlots()
    {
        // เคลียร์ของเก่า
        foreach (Transform child in slotPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // สร้างใหม่
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, slotPanel.transform);
        }
    }
}