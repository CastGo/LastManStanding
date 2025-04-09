using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    private ItemSlot selectedSlot;

    public GameObject inventoryPanel;
    public GameObject slotPanel;
    public GameObject slotPrefab;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
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

        if (Input.GetMouseButtonDown(0) && isInventoryOpen)
        {
            DetectSlotClick();
        }
    }
    private void DetectSlotClick()
    {
        Vector2 mousePos = Input.mousePosition;
        bool slotClicked = false;

        foreach (Transform slotTransform in slotPanel.transform)
        {
            RectTransform rect = slotTransform.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
            {
                ItemSlot slot = slotTransform.GetComponent<ItemSlot>();

                // ✅ ยกเลิกช่องก่อนหน้า
                if (selectedSlot != null)
                    selectedSlot.Deselect();

                selectedSlot = slot;
                selectedSlot.Select();

                // ✅ ไม่ว่าใน slot จะมีไอเทมหรือไม่ ก็แสดงหรือเคลียร์ได้
                if (slot.currentItem != null)
                    ShowItemInfo(slot);
                else
                    ClearItemInfo();

                slotClicked = true;
                break;
            }
        }

        if (!slotClicked)
        {
            ClearItemInfo();
            if (selectedSlot != null)
            {
                selectedSlot.Deselect();
                selectedSlot = null;
            }
        }
    }
    public bool AddItem(GameObject itemPrefab)
    {
        //Look for empty slot
        foreach (Transform slotTranform in slotPanel.transform)
        {
            ItemSlot slot = slotTranform.GetComponent<ItemSlot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTranform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }
    public List<InventorySaveData> GetInventoryItems()
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
        foreach (Transform slotTransform in slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                ItemSlot slot = slotPanel.transform.GetChild(data.slotIndex).GetComponent<ItemSlot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
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
    private void ShowItemInfo(ItemSlot slot)
    {
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null)
            {
                itemNameText.text = item.itemName;
                itemDescriptionText.text = item.itemDescription;
            }
        }
    }

    private void ClearItemInfo()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
    }
}