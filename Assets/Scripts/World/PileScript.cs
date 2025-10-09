using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PileScript : MonoBehaviour
{
    //list of things needed in this script
    /*
     * max stretch for x/y
     * invert value for x/y
     * do the stretching
     * detect only consumable layer
     * get bigger when blobbed, glorp faster for a period of time
     */

    [SerializeField] Transform pile_model;

    [SerializeField] float h_max;
    [SerializeField] float v_max;

    private int h_invert;
    private int v_invert;

    private float h_min;
    private float v_min;

    public float animation_speed;

    private void Start()
    {
        h_invert = 1;
        v_invert = 1;

        h_min = 1f;
        v_min = 1.5f;

        animation_speed = 0.001f;

        //ignore collision with the player
        Physics.IgnoreCollision(GetComponent<BoxCollider>(), GameObject.Find("Collider").GetComponent<BoxCollider>(), true);
    }

    private void Update()
    {
        //inverting the direction if outside of limits
        if(transform.localScale.x > h_max || transform.localScale.x < h_min)
        {
            h_invert = -h_invert;
        }
        if (transform.localScale.y > v_max || transform.localScale.y < v_min)
        {
            v_invert = -v_invert;
        }

        //change scale
        Vector3 scale_diff = new Vector3((h_invert * animation_speed), (v_invert * animation_speed), (h_invert * animation_speed));
        transform.localScale += scale_diff;

        //for when animation speed increased
        if (animation_speed > 0.001f)
        {
            animation_speed -= 0.00001f;
        }
    }

    //for when object collides
    private void OnTriggerEnter(Collider other)
    {
        //is garunteed to be the right object

        //delete the object
        Destroy(other.gameObject);

        //make sure to allow the player to jump again if they need to
        PlayerScript.canJump = true;

        //make it *bigger*
        h_max += 0.3f;
        v_max += 0.3f;

        //make sure it can't be as small again
        h_min += 0.1f;
        v_min += 0.1f;

        //set the scale to the new minimum so it doesnt break
        transform.localScale = new Vector3(h_min, v_min, h_min);
        
        //make the animation temporarily faster
        animation_speed = 0.005f;

        //make sure it only increases first
        v_invert = 1;
        h_invert = 1;
    }
}
