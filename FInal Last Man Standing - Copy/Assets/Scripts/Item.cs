using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string itemName;
    public string itemDescription;

    // ✅ เพิ่มจำนวน stack
    public int quantity = 1;

    // ✅ ไอเทมนี้สามารถ stack ได้หรือไม่
    public bool isStackable = false;

    // ✅ stack ได้สูงสุดกี่อัน
    public int maxStack = 10;
}
