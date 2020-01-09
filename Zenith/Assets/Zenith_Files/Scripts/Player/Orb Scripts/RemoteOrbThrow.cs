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
    public Transform orbEmitter;
    public float throwSpeed = 5.0f;

    [HideInInspector]
    public bool hasBeenFired = false;

    private Transform mainCameraTrans;

    // Start is called before the first frame update
    void Start()
    {
        mainCameraTrans = GetComponentInChildren<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Add stanima check/update when stanima is put into it's own script

        if (Input.GetButton("PullOrb"))
        {
            if(hasBeenFired == false)
            {
                SpawnOrb(pullOrbPrefab);
                hasBeenFired = true;
            }

            //Debug.Log("PullOrb Pressed");
        }
        else if(Input.GetButton("PushOrb"))
        {
            if (hasBeenFired == false)
            {
                SpawnOrb(pushOrbPrefab);
                hasBeenFired = true;
            }
        }
    }

    private void SpawnOrb(GameObject orbPrefab)
    {
        //TODO: Add delay to spawn after animation plays

        GameObject newOrb = Instantiate(orbPrefab, orbEmitter.position, orbEmitter.rotation, null);
        newOrb.GetComponent<RemoteOrbMine>().parent = this;

        Vector3 dir = mainCameraTrans.forward; //Dir player is facing INCLUDING rotation
        newOrb.GetComponent<Rigidbody>().AddForce(dir * throwSpeed, ForceMode.Impulse);
    }
}
