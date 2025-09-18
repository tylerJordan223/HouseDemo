using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*

    Purpose: Main Character Control Script
    Author: King_Korby
    Date Created: 7/8/2025

    Legend:
    "//" - Regular Comment
    "//**" - To Be Added
    "*temp*" - Temporary line

*/

public class PlayerScript : MonoBehaviour
{
    //all variables for the parts of the model
    private Transform trans;
    private Rigidbody rb;
    private BoxCollider collider;

    //graphic stuff
    private Animator anim;
    private SpriteRenderer sr;

    //character mechanics
    private float health;
    private float playerSpeed;
    private float jumpHeight;

    //Saved Objects
    public GameObject heldObject;
    public GameObject currentRoom;
    public List<GameObject> itemList = new List<GameObject>();

    //flags
    public bool canControl;
    public bool holdingItem;
    public bool _jumping;

    //movement
    public Vector3 inputVector;
    public Vector3 lastInputVector;

    private void Start()
    {
        //object variables
        trans = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();

        //graphics
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        //character mechanics
        health = 100f;
        playerSpeed = 5f;
        jumpHeight = 10f;

        //saved objects
        heldObject = null;
        currentRoom = null;

        //flags
        canControl = true;
        holdingItem = false;
        _jumping = false;
    }

    //use for anything non-movement related like math
    private void Update()
    {
        if(canControl)
        {
            //update input
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            //update last input if actual input and not 0
            // CURRENTLY SET UP FOR 4 DIRECTIONAL, IF HORIZONTAL THEN USE THAT
            if ( (inputVector.x != 0 || inputVector.z != 0) && (inputVector.x == 0 || inputVector.z == 0) )
            {
                lastInputVector = inputVector;
            }

            //set the jump
            if(Input.GetKeyDown(KeyCode.Space) && !_jumping)
            {
                _jumping = true;
            }

            //place/pickup item
            if(holdingItem)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    //Setting object's parent to current room
                    heldObject.transform.SetParent(currentRoom.transform.Find("Items").transform, true);
                    heldObject.transform.localScale = Vector3.one;
                    heldObject.transform.position = new Vector3(heldObject.transform.position.x, heldObject.transform.position.y - (collider.bounds.size.y / 2), heldObject.transform.position.z);
                    heldObject = null;
                    holdingItem = false;
                }
            }else
            {
                if(itemList.Count != 0 && Input.GetKeyDown(KeyCode.E))
                {
                    GameObject item = itemList[0];
                    itemList.RemoveAt(0);
                    item.transform.SetParent(trans.Find("HeldItem"), false);
                    item.transform.localPosition = Vector3.zero;
                    heldObject = item;
                    holdingItem = true;
                }
            }
        }

        //kill when out of hp
        if (health <= 0)
        {
            canControl = false;
            health = 0;

            StartCoroutine(Death());
        }

        //update Animation
        UpdateAnimation();
    }

    //use for anything movement related
    private void FixedUpdate()
    {
        //normalize for the sake of diagonal not double speed
        inputVector.Normalize();

        //update on speed if alive, stop if else
        if(health > 0)
        {

            rb.velocity = new Vector3(inputVector.x * playerSpeed, rb.velocity.y, inputVector.z * playerSpeed);

            //handling the jump
            if(Input.GetKey(KeyCode.Space) && !_jumping && rb.velocity.y <= 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
                _jumping = true;
            }
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

    }

    //handle the death sequence
    IEnumerator Death()
    {
        //** set animation to dead
        //** wait for animation to end
        Application.Quit();
        /*temp*/yield return null;
    }

    //Update the Animation
    private void UpdateAnimation()
    {
        //** Update all the animation
    }

    //to handle collisions

    private void OnCollisionStay(Collision collision)
    {
        if(collision.transform.tag == "Ground")
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, LayerMask.GetMask("Ground")))
            {
                if (_jumping)
                {
                    _jumping = false;
                }
            }
        }
    }

}
