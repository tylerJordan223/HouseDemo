using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
    //single instance of player initialization
    #region initialization
    private static GameObject player_instance;
    #endregion

    //all variables for the parts of the model
    private Transform trans;
    private Rigidbody rb;
    private BoxCollider col;

    //graphic stuff
    private Animator anim;
    private static GameObject player_sprite;

    //character mechanics
    private float health;
    private float playerSpeed;
    private float jumpHeight;

    //Saved Objects
    public GameObject heldObject;
    public GameObject currentRoom;
    public List<GameObject> itemList = new List<GameObject>();

    //flags
    public static bool canControl;
    public static bool canJump;
    public bool holdingItem;
    public bool _jumping;

    //movement
    public static bool swapped;
    public static float inv_x;
    public static float inv_y;
    public Vector3 inputVector;
    public static Vector3 lastInputVector;

    //other public variables
    public static Transform player_trans;
    public static Transform held_object_trans;

    private void Start()
    {
        //making sure theres one player
        if (player_instance != null && player_instance != this.gameObject)
        {
            Destroy(this.gameObject);
        }
        else
        {
            player_instance = this.gameObject;
            player_trans = this.transform;
            held_object_trans = this.transform.Find("HeldObject");
            Debug.Log(held_object_trans.name);
        }

        //object variables
        trans = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();

        //graphics
        anim = GetComponent<Animator>();
        player_sprite = transform.Find("PlayerSprite").gameObject;

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
        swapped = false;
        canJump = true;
        inv_x = 1;
        inv_y = 1;
    }

    //use for anything non-movement related like math
    private void Update()
    {
        if(canControl)
        {
            //update input, swaps them based on camera
            if(!swapped)
            {
                inputVector = new Vector3(Input.GetAxisRaw("Horizontal") * inv_x, 0f, Input.GetAxisRaw("Vertical") * inv_y);
            }
            else
            {
                inputVector = new Vector3(Input.GetAxisRaw("Vertical") * inv_y, 0f, Input.GetAxisRaw("Horizontal") * inv_x);
            }

            //update last input if actual input and not 0
            // CURRENTLY SET UP FOR 4 DIRECTIONAL, IF HORIZONTAL THEN USE THAT
            if ( (inputVector.x != 0 || inputVector.z != 0) && (inputVector.x == 0 || inputVector.z == 0) )
            {
                lastInputVector = inputVector;
            }

            //place/pickup item
            if(holdingItem)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    //Setting object's parent to current room
                    heldObject.transform.SetParent(currentRoom.transform.Find("Items").transform, true);
                    heldObject.transform.localScale = Vector3.one;
                    heldObject.transform.position = new Vector3(heldObject.transform.position.x, heldObject.transform.position.y - (col.bounds.size.y / 2), heldObject.transform.position.z);
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
        else
        {
            inputVector = new Vector3(0f, 0f, 0f);
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
            if(Input.GetKey(KeyCode.Space) && !_jumping && rb.velocity.y <= 0 && canJump)
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

    //stop the player
    public static void stopPlayer()
    {
        canControl = false;
    }

    public static void startPlayer()
    {
        canControl = true;
    }

    //orientation shifting
    public static void shiftControls(float x, float y)
    {
        swapped = !swapped;
        inv_x = x;
        inv_y = y;
    }

    //performs the turn with the camera 
    public static IEnumerator performRotation(float turn_direction)
    {
        //get the values for the rotation
        float start = player_sprite.transform.localRotation.eulerAngles.y;
        float goal_rotation = player_sprite.transform.localRotation.eulerAngles.y + (90 * turn_direction) + (360 * 3); //do 3 spins on it holy woah

        //actually do all the rotation
        float time = 0f;
        while (time < 1f)
        {
            Vector3 pos_rotate = new Vector3(0f, Mathf.Lerp(start, goal_rotation, time), 0);

            player_sprite.transform.SetLocalPositionAndRotation(player_sprite.transform.localPosition, Quaternion.Euler(pos_rotate));

            time += Time.deltaTime * 2f;

            yield return null;
        }

        //set the final rotation
        player_sprite.transform.SetLocalPositionAndRotation(player_sprite.transform.localPosition, Quaternion.Euler(0f, goal_rotation, 0f));
    }

}
