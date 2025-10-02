using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [SerializeField] double mass;
    public bool isHeld;
    private Rigidbody rb;

    [SerializeField] Collider phys_collider;

    [Header("Parabola")]
    public float arcHeight = 1f;
    public Transform player_held;
    private bool onPlayer;
    private Collider player_collider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        isHeld = false;
        onPlayer = false;
        player_collider = GameObject.Find("Player").GetComponent<Collider>();
    }

    private void Update()
    {
        if(isHeld && Input.GetKeyDown(KeyCode.F))
        {
            this.transform.SetParent(null);
            rb.isKinematic = false;
            rb.mass = 10;
            isHeld = false;
            PlayerScript.canJump = true;

            //only throw if moving
            if(PlayerScript.inputVector != new Vector3(0f,0f,0f))
            {
               rb.AddForce(PlayerScript.lastInputVector * 100f, ForceMode.Impulse);
            }
        }

        if (onPlayer && !isHeld && Input.GetKeyDown(KeyCode.F))
        {
            isHeld = true;
            player_held = PlayerScript.held_object_trans;
            this.transform.SetParent(player_held);
            rb.isKinematic = true;
            rb.mass = 0;
            Physics.IgnoreCollision(phys_collider, player_collider, true);

            StartCoroutine(PickedUp());
        }

        if(!onPlayer && !isHeld && Physics.GetIgnoreCollision(phys_collider, player_collider))
        {
            Physics.IgnoreCollision(phys_collider, player_collider, false);
        }
    }

    public IEnumerator PickedUp()
    {
        //pause the player
        PlayerScript.canControl = false;
        PlayerScript.canJump = false;

        Transform startPos = this.transform;

        //calcluate the path
        float time = 0f;
        while(time < 1f)
        {

            Vector3 nextPos = new Vector3(Mathf.Lerp(
                transform.position.x, player_held.position.x, time), 
                Mathf.Lerp(transform.position.y, player_held.position.y, time), 
                Mathf.Lerp(transform.position.z, player_held.position.z, time)
                );

            transform.position = nextPos;

            time += Time.deltaTime * 2f;

            yield return null;
        }

        transform.position = player_held.position;

        //give player control back and break
        PlayerScript.canControl = true;
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayer = true;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onPlayer = false;
        }
    }
}
