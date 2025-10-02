using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    //handling the wall fading
    private WallScript currentWall;

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

        #region raycast-for-wall-fade

        //raycast for wall
        RaycastHit[] walls = new RaycastHit[10];
        //actually performing the raycast towards the player
        Physics.RaycastNonAlloc(new Ray(transform.position, (PlayerScript.player_trans.position - transform.position).normalized), walls);

        //sort the list based on distance
        var sorted_walls = walls.OrderByDescending(ch => ch.distance).ToList();
        sorted_walls.Reverse();

        //if it hits anything
        if (sorted_walls.Count > 0)
        {
            //go through all the hits
            for(int i = 0; i < sorted_walls.Count; i++)
            {
                //gets rid of empty space in array
                if (sorted_walls[i].distance != 0)
                {
                    //if its a wall
                    if (sorted_walls[i].transform.CompareTag("Wall"))
                    {
                        //for the first wall intersected
                        if(currentWall == null)
                        {
                            currentWall = sorted_walls[i].transform.GetComponent<WallScript>();
                            currentWall.fading = true;
                        }

                        //if it hits current wall and fading is false
                        if(sorted_walls[i].transform.GetComponent<WallScript>() == currentWall && currentWall.fading == false)
                        {
                            currentWall.fading = true;
                        }

                        //if its a different wall
                        if (!(sorted_walls[i].transform.GetComponent<WallScript>() == currentWall))
                        {
                            //swap the walls
                            currentWall.fading = false;
                            currentWall = sorted_walls[i].transform.GetComponent<WallScript>();
                            currentWall.fading = true;
                        }

                        //end the loop since this was the first wall it found no matter the outcome
                        i = sorted_walls.Count;
                    }
                    else if (sorted_walls[i].transform.CompareTag("Player")) //if it hits the player first then it needs to end early
                    {

                        //also if there was a current faded wall now unfade it
                        if (currentWall != null)
                        {
                            if (currentWall.fading)
                            {
                                currentWall.fading = false;
                            }
                        }
                        //end loop
                        i = sorted_walls.Count;
                    }
                }

            }
        }
        else
        {
            //if it hits no walls that means no wall needs to fade out
            currentWall.fading = false;
        }

        #endregion raycast-for-wall-fade

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

        cam.transform.SetLocalPositionAndRotation(new Vector3(orientations[state][0], cam.transform.localPosition.y, orientations[state][1]), Quaternion.Euler(15f, goal_rotation, 0f));

        //restart the player and end the loop
        PlayerScript.startPlayer();
        rotating = false;
    }


}
