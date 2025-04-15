using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;

    public int damage;
    public int maxHP;
    public int currentHP;
    public int maxEnergy;
    public int currentEnergy;

    // ✅ ค่าพื้นฐานเริ่มต้น
    private int defaultLevel;
    private int defaultDamage;
    private int defaultMaxHP;
    private int defaultMaxEnergy;

    public AudioClip damageSound;
    private AudioSource audioSource;

    void Awake()
    {
        // เซฟค่าพื้นฐานที่ตั้งใน Inspector ไว้
        defaultLevel = unitLevel;
        defaultDamage = damage;
        defaultMaxHP = maxHP;
        defaultMaxEnergy = maxEnergy;
        audioSource = GetComponent<AudioSource>();
    }

    // ✅ เรียกตอนเริ่ม New Game
    public void ResetToDefault()
    {
        unitLevel = defaultLevel;
        damage = defaultDamage;

        maxHP = defaultMaxHP;
        currentHP = 25;

        maxEnergy = defaultMaxEnergy;
        currentEnergy = 10;
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        return currentHP <= 0;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public bool UseEnergy(int cost)
    {
        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            return true; // ใช้สำเร็จ
        }

        return false; // energy ไม่พอ
    }

    public void GainEnergy(int amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }

    public IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sprite.color = originalColor;
        }
    }
}

