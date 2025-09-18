using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    //Script used for the rotating of camera for player

    //IMPORTANT NOTE//
    //idk why but in order for the rotation to work properly the Q needs a -90 value when looping back to 0

    //actual camera
    public CinemachineVirtualCamera cam;

    //assisting values to send to the player
    public float swapped; // 0 = no, 1 = yes
    public float inverted;
    public float turn_direction;

    //flags
    private bool rotating;

    //testers
    [SerializeField] public float E_rotation;
    [SerializeField] public float Q_rotation;

    //list of all the camera orientations in order:
    // [0] = X position
    // [1] = Z position
    // [2] = x invert
    // [3] = y invert
    private int state;
    private static List<float[]> orientations = new List<float[]>
    {
        new float[4] {0,-6, 1, 1},
        new float[4] { -6,0, -1, 1},
        new float[4] { 0,6, -1, -1},
        new float[4] { 6,0, 1, -1}
    };


    private void Start()
    {

        swapped = 0;
        inverted = 1;
        state = 0;
        rotating = false;
    }

    private void Update()
    {

        //rotation states
        if (Input.GetKeyDown(KeyCode.E) && !rotating)
        {
            if (state == 0)
            {
                state = orientations.Count - 1;
                cam.transform.SetLocalPositionAndRotation(cam.transform.localPosition, Quaternion.Euler(new Vector3(cam.transform.localRotation.eulerAngles.x, E_rotation, cam.transform.localRotation.eulerAngles.z)));
            }
            else
            {
                state -= 1;
            }

            turn_direction = -1;
            StartCoroutine(RotateCamera());
        }

        if (Input.GetKeyDown(KeyCode.Q) && !rotating)
        {
            if (state == orientations.Count - 1)
            {
                state = 0;
                cam.transform.SetLocalPositionAndRotation(cam.transform.localPosition, Quaternion.Euler(new Vector3(cam.transform.localRotation.eulerAngles.x, Q_rotation, cam.transform.localRotation.eulerAngles.z)));
            }
            else
            {
                state += 1;
            }

            turn_direction = 1;
            StartCoroutine(RotateCamera());
        }

    }

    //lerp should be start/end and then the percentage between them 

    IEnumerator RotateCamera()
    {
        //stop the player from moving
        PlayerScript.stopPlayer();
        //can't rotate again
        rotating = true;

        //get the current camera orientation
        float[] start = {cam.transform.localPosition.x, cam.transform.localPosition.z, cam.transform.localRotation.eulerAngles.y};
    
        //math out the rotation
        float goal_rotation = cam.transform.localRotation.eulerAngles.y + (90 * turn_direction);

        //update the player
        PlayerScript.shiftControls(orientations[state][2], orientations[state][3]);

        //actually do all the rotation, and also have the player do it
        StartCoroutine(PlayerScript.performRotation(turn_direction));
        float time = 0f;
        while (time < 1f)
        {
            Vector3 pos_move = new Vector3(Mathf.Lerp(start[0], orientations[state][0], time), cam.transform.localPosition.y, Mathf.Lerp(start[1], orientations[state][1], time));
            Vector3 pos_rotate = new Vector3(15f, Mathf.Lerp(start[2], goal_rotation, time), 0);

            cam.transform.SetLocalPositionAndRotation(pos_move, Quaternion.Euler(pos_rotate));

            time += Time.deltaTime * 2f;

            yield return null;
        }

        //restart the player and end the loop
        PlayerScript.startPlayer();
        rotating = false;
    }


}
