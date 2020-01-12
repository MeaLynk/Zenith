//--------------------------------------------------------------
//  Remote Orb Throw Script
//  By: Kameron Brodhagen
//
//  Creates and throws the push and pull orbs
//--------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteOrbThrow : MonoBehaviour
{
    public GameObject pullOrbPrefab;
    public GameObject pushOrbPrefab;

    private bool hasBeenPressed = false; //NOO HOLD

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("PullOrb"))
        {
            if(hasBeenPressed == false)
            {
                SpawnOrb(pullOrbPrefab);
                hasBeenPressed = true;
            }

            Debug.Log("PullOrb Pressed");
        }
        else if(Input.GetButton("PushOrb"))
        {

        }
    }

    private void SpawnOrb(GameObject orbPrefab)
    {

    }
}
