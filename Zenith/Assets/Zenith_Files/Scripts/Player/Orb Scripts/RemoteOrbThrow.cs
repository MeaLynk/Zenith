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
    public enum PlayerType { PLAYER1, PLAYER2 }

    public GameObject pullOrbPrefab;
    public GameObject pushOrbPrefab;
    public Transform orbEmitter;
    public float throwSpeed = 5.0f;

    [HideInInspector]
    public bool hasBeenFired = false;

    private Transform mainCameraTrans;
    private PlayerType currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        mainCameraTrans = GetComponentInChildren<Camera>().transform;

        if(gameObject.tag == "Player1")
        {
            currentPlayer = PlayerType.PLAYER1;
        }
        else if(gameObject.tag == "Player2")
        {
            currentPlayer = PlayerType.PLAYER2;
        }

        Debug.Log(gameObject.name + " has been givin: " + currentPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Add stanima check/update when stanima is put into it's own script

        float pullAxis = 0;
        float pushAxis = 0;

        if(currentPlayer == PlayerType.PLAYER1)
        {
            pullAxis = Input.GetAxis("PlayerOne_PullOrb");
            pushAxis = Input.GetAxis("PlayerOne_PushOrb");
        }
        else if(currentPlayer == PlayerType.PLAYER2)
        {
            pullAxis = Input.GetAxis("PlayerTwo_PullOrb");
            pushAxis = Input.GetAxis("PlayerTwo_PushOrb");
        }

        if (pullAxis > 0)
        {
            if(hasBeenFired == false)
            {
                SpawnOrb(pullOrbPrefab);
                hasBeenFired = true;
            }

            //Debug.Log("PullOrb Pressed");
        }
        else if(pushAxis > 0)
        {
            if (hasBeenFired == false)
            {
                SpawnOrb(pushOrbPrefab);
                hasBeenFired = true;
            }
        }
    }

    //Spawns the orb that the player fired
    private void SpawnOrb(GameObject orbPrefab)
    {
        //TODO: Add delay to spawn after animation plays

        GameObject newOrb = Instantiate(orbPrefab, orbEmitter.position, orbEmitter.rotation, null);
        newOrb.GetComponent<RemoteOrbMine>().parent = this;

        Vector3 dir = mainCameraTrans.forward; //Dir player is facing INCLUDING rotation
        newOrb.GetComponent<Rigidbody>().AddForce(dir * throwSpeed, ForceMode.Impulse);
    }

    private void OnGUI()
    {
        //GUI.Box(new Rect(new Rect(10, 10, 200, 30)), "Can Fire Orb: " + !hasBeenFired);
    }
}
