using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZombieMovement : MonoBehaviour
{
    public bool facingLeft;
    public float moveSpeed;
    public bool isWalking;
    public float walkTime;
    public float walkCounter;
    public float waitTime;
    public float waitCounter;
    private bool moveRight = true;
    private bool moveLeft;

    public bool startDirection;

    private Rigidbody2D rb;
    private Animator anim;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        waitCounter = waitTime;
        walkCounter = walkTime;
        //moveLeft = true;
        //moveRight = false;

        //bool startDirection = Random.Range(0, 2) == 0;

        if (startDirection)
        {
            moveLeft = true;
            moveRight = false;
        }
        else
        {
            moveRight = true;
            moveLeft = false;
            
            Flip();
        }
    }

    
    void Update()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;
            if (moveRight == true)
            {
                MoveRight();
            }
            if (moveLeft == true)
            {
                MoveLeft();
            }
            if (walkCounter <= 0)
            {
                isWalking = false;
                waitCounter = waitTime;
            }
        }
        else
        { 
            waitCounter -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            anim.SetBool("Move", false);
            if(waitCounter <= 0) 
            {
                ChooseDirection();
            }
        }
    }


    public void ChooseDirection()
    {
        if(moveRight == true)
        {
            moveLeft = true;
            moveRight = false;

            Flip();
        }
        else if(moveLeft == true) 
        {
            moveRight = true;
            moveLeft = false;

            Flip();
        }

        isWalking = true;
        walkCounter = walkTime;
    }

    public void MoveRight()
    {
        rb.velocity = new Vector2(moveSpeed, 0);
        anim.SetBool("Move", true);
    }

    public void MoveLeft()
    {
        rb.velocity = new Vector2(-moveSpeed, 0);
        anim.SetBool("Move", true);
    }

    public void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.instance.isLoadingFromSave)
        {
            GameManager.instance.LoadTurnBase(other.transform, gameObject);
        }
    }
}
