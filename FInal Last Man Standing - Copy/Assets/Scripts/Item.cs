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
    public int restoreHP = 0;
    public int restoreEnergy = 0;

    public void Use()
    {
        Debug.Log("Used: " + itemName);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Unit playerUnit = player.GetComponent<Unit>();
            if (playerUnit != null)
            {
                if (restoreHP > 0)
                {
                    playerUnit.currentHP = Mathf.Min(playerUnit.currentHP + restoreHP, playerUnit.maxHP);
                    Debug.Log("Recovered " + restoreHP + " HP!");
                }

                if (restoreEnergy > 0)
                {
                    playerUnit.currentEnergy = Mathf.Min(playerUnit.currentEnergy + restoreEnergy, playerUnit.maxEnergy);
                    Debug.Log("Recovered " + restoreEnergy + " Energy!");
                }
            }
        }
    }
}
