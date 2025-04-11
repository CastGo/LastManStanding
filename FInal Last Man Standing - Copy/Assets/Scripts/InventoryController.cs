using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    [HideInInspector] public ItemSlot selectedSlot;

    public GameObject menuPanel;
    public GameObject mapPanel;
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

        // ✅ ตรวจสอบคลิกซ้ายหรือขวา แล้วส่งค่าไปยัง DetectSlotClick
        if (Input.GetMouseButtonDown(0) && isInventoryOpen)
        {
            DetectSlotClick(false); // คลิกซ้าย
        }
        else if (Input.GetMouseButtonDown(1) && isInventoryOpen)
        {
            DetectSlotClick(true); // คลิกขวา
        }
    }
    private void DetectSlotClick(bool isRightClick)
    {
        Vector2 mousePos = Input.mousePosition;
        bool slotClicked = false;

        foreach (Transform slotTransform in slotPanel.transform)
        {
            RectTransform rect = slotTransform.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
            {
                ItemSlot slot = slotTransform.GetComponent<ItemSlot>();

                if (selectedSlot != null)
                    selectedSlot.Deselect();

                selectedSlot = slot;
                selectedSlot.Select();

                if (slot.currentItem != null)
                {
                    if (isRightClick)
                        UseItem(slot); // 👈 เพิ่มตรงนี้
                    else
                        ShowItemInfo(slot);
                }
                else
                {
                    ClearItemInfo();
                }

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
        Item newItemData = itemPrefab.GetComponent<Item>();

        if (newItemData.isStackable)
        {
            // 🔁 หา slot ที่ stack ได้
            foreach (Transform slotTransform in slotPanel.transform)
            {
                ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
                if (slot.currentItem != null)
                {
                    Item existingItem = slot.currentItem.GetComponent<Item>();
                    if (existingItem.ID == newItemData.ID && existingItem.quantity < existingItem.maxStack)
                    {
                        existingItem.quantity++;
                        slot.UpdateStackText();
                        return true;
                    }
                }
            }
        }

        // ✅ หา slot ว่าง
        foreach (Transform slotTransform in slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Item itemComponent = newItem.GetComponent<Item>();
                itemComponent.quantity = 1;

                slot.currentItem = newItem;
                slot.UpdateStackText();
                return true;
            }
        }

        // ❌ เต็มทุกช่อง หรือ stack เต็มหมด
        Debug.Log("Inventory is full or cannot stack this item.");
        return false;
    }
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform itemslotTranfrom in slotPanel.transform)
        {
            ItemSlot itemSlot = itemslotTranfrom.GetComponent<ItemSlot>();
            if (itemSlot.currentItem != null)
            {
                Item item = itemSlot.currentItem.GetComponent<Item>();

                // ❗ ป้องกันไม่ให้บันทึก item ที่หมดแล้ว (quantity <= 0)
                if (item.quantity > 0)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = itemslotTranfrom.GetSiblingIndex(),
                        quantity = item.quantity
                    });
                }
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

                    Item itemComponent = item.GetComponent<Item>();
                    itemComponent.quantity = data.quantity;

                    slot.currentItem = item;
                    slot.UpdateStackText();
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
    private void UseItem(ItemSlot slot)
    {
        if (slot.currentItem == null) return;

        Item item = slot.currentItem.GetComponent<Item>();
        if (item != null && item.isUsable)
        {
            item.Use();

            // Sync ค่า HP/Energy กับ GameManager
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Unit unit = player.GetComponent<Unit>();
                GameManager.instance.savedHP = unit.currentHP;
                GameManager.instance.savedEnergy = unit.currentEnergy;
            }

            item.quantity--;

            if (item.quantity <= 0)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }

            slot.UpdateStackText();
            ShowItemInfo(slot);
        }
    }
    public void ShowItemInfo(ItemSlot slot)
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