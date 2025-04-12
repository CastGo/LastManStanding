//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private int faceDirection;
    private bool facingRight;
    public float speed;
    public float faceValu;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        Unit unit = GetComponent<Unit>();
        if (GameManager.instance != null)
        {
            // ถ้ายังไม่มีข้อมูลใน GameManager → เซฟค่าเริ่มต้นจาก Player
            if (GameManager.instance.savedHP <= 0)
            {
                GameManager.instance.savedHP = unit.currentHP;
                GameManager.instance.savedMaxHP = unit.maxHP;
                GameManager.instance.savedEnergy = unit.currentEnergy;
                GameManager.instance.savedMaxEnergy = unit.maxEnergy;
                GameManager.instance.savedDamage = unit.damage;
                GameManager.instance.savedLevel = unit.unitLevel;
                GameManager.instance.savedName = unit.unitName;
            }
            else
            {
                // มีข้อมูลแล้ว → โหลดกลับมาที่ Player
                unit.currentHP = GameManager.instance.savedHP;
                unit.maxHP = GameManager.instance.savedMaxHP;
                unit.currentEnergy = GameManager.instance.savedEnergy;
                unit.maxEnergy = GameManager.instance.savedMaxEnergy;
                unit.damage = GameManager.instance.savedDamage;
                unit.unitLevel = GameManager.instance.savedLevel;
                unit.unitName = GameManager.instance.savedName;
            }
        }
    }
    public void RefreshStatsFromGameManager()
    {
        Unit unit = GetComponent<Unit>();

        unit.currentHP = GameManager.instance.savedHP;
        unit.maxHP = GameManager.instance.savedMaxHP;
        unit.currentEnergy = GameManager.instance.savedEnergy;
        unit.maxEnergy = GameManager.instance.savedMaxEnergy;
        unit.damage = GameManager.instance.savedDamage;
        unit.unitLevel = GameManager.instance.savedLevel;
        unit.unitName = GameManager.instance.savedName;
    }
    // Update is called once per frame
    void Update()
    {
        faceValu = Input.GetAxisRaw("Horizontal");
        if(faceValu < 0 && !facingRight)
        {
            Flip();
            faceDirection = 1;
        }
        else if(faceValu > 0 && facingRight)
        {
            Flip();
            faceDirection = -1;
        }

        rb.velocity = new Vector2(faceValu * speed, rb.velocity.y);

        if(faceValu > 0 || faceValu < 0 )
        {
            anim.SetBool("Move", true);
        }
        else
        {
            anim.SetBool("Move", false);
        }
    }

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
