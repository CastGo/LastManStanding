using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string itemName;
    public string itemDescription;

    public int quantity = 1;
    public bool isStackable = false;
    public int maxStack = 10;

    public bool isUsable = false;

    public void Use()
    {
        Debug.Log("Used item: " + itemName);
        // ใส่ logic การใช้งานไอเทมจริงๆ ตรงนี้ (เช่น Heal, เพิ่มพลัง, เป็นต้น)
    }
}
