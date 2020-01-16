//--------------------------------------------------------------
//  Remote Orb Trigger Script
//  By: Kameron Brodhagen
//
//  Script attached to the trigger object as the child of the orb to get objects to be affected
//--------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbTriggerScript : MonoBehaviour
{
    RemoteOrbMine parent;

    private void Start()
    {
        parent = GetComponentInParent<RemoteOrbMine>();
    }
    private void OnTriggerEnter(Collider other)
    { 
        if (other.tag == "Grabbable Object" || other.tag == "Runt" || other.tag == "Adolescent" || other.tag == "Player")
        {
            parent.AddObj(other.gameObject);
            //Debug.Log("Added new obj for push/pull");
        }
    }
}
