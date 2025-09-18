using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Datamanager : MonoBehaviour
{
    //This script handles all of the global information and provides functions that need to be accessed by all

    #region initialization
    // CHECKING TO MAKE SURE THERE ARE NOT DUPLICATES OF THIS OBJECT! //
    private static GameObject dataManager_instance;

    //check before anything is run to make sure there isnt one of these already
    private void Start()
    {
        //making sure this doesnt double
        if (dataManager_instance != null && dataManager_instance != this.gameObject)
        {
            Destroy(this.gameObject);
        }
        else
        {
            dataManager_instance = this.gameObject;
            //makes sure this stays
            DontDestroyOnLoad(this.gameObject);
        }
    }
    #endregion
}
