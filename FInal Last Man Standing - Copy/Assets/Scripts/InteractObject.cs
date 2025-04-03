using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class InteractObject : MonoBehaviour
{
    [SerializeField]
    PolygonCollider2D map;
    CinemachineConfiner confiner;
    public Transform teleportDestination;

    public GameObject interact;
    public GameObject interactLight;
    private bool playerInRange;

    void Start()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && playerInRange)
        {      
                if (interact.CompareTag("savepoint"))
                {
                    SaveController saveController = FindObjectOfType<SaveController>();
                    if (saveController != null) 
                    { 
                        saveController.SaveGame();
                    }
                }
                if (interact.CompareTag("item"))
                {
                    
                }
                if (interact.CompareTag("door"))
                {
                    confiner.m_BoundingShape2D = map;

                if (teleportDestination != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = teleportDestination.position;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = true;
            interactLight.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = false;
            interactLight.SetActive(false);
        }
    }
    
}
