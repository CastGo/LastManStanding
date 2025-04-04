using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Slider hpSlider;
    public Slider energySlider;
    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        levelText.text = "Lv." + unit.unitLevel;

        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;

        energySlider.maxValue = unit.maxEnergy;
        energySlider.value = unit.currentEnergy;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;
    }

    public void SetEnergy(int energy)
    {
        energySlider.value = energy;
    }
}
