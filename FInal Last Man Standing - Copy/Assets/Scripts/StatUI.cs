using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    public Slider HpBar;
    public Slider EnergyBar;

    private Unit playerUnit;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerUnit = player.GetComponent<Unit>();

            HpBar.maxValue = playerUnit.maxHP;
            EnergyBar.maxValue = playerUnit.maxEnergy;

            UpdateUI();
        }
    }

    void Update()
    {
        if (playerUnit != null)
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        HpBar.value = playerUnit.currentHP;
        EnergyBar.value = playerUnit.currentEnergy;
    }
}
