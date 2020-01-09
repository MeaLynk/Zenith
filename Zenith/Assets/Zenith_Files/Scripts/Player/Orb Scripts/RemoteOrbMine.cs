//--------------------------------------------------------------
//  Remote Orb Throw Script
//  By: Kameron Brodhagen
//
//  Script attached to the orb keeping track of it's status
//--------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteOrbMine : MonoBehaviour
{
    public enum Mode { PUSH, PULL }

    public Mode currentMode;
    public float throwTimer = 10;
    public float activeTimer = 5;

    [HideInInspector]
    public RemoteOrbThrow parent;

    private bool isActive = false;
    private float currentTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = throwTimer; //How long the orb has in order to hit something (in order to prevent them from staying in scene if nothing is hit)
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive == true)
        {

        }

        if(currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
        }
        else
        {
            parent.hasBeenFired = false; //Resets throw
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Orb-able Area")
        {
            Debug.Log("Orb Successfully Activated.");
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            isActive = true;
            currentTimer = activeTimer; //Resets timer for active time legnth
        }
        else
        {
            Debug.Log("Orb Failed to Activate.");
            parent.hasBeenFired = false; //Resets throw
            Destroy(gameObject);
        }
    }
}
