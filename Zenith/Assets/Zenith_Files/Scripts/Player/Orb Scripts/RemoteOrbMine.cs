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
    public GameObject activeCol;
    public float pushPower = 10;
    public float pullPower = 10;
    public float throwTimer = 10;
    public float activeTimer = 5;

    [HideInInspector]
    public RemoteOrbThrow parent;

    private SphereCollider activeCollider;
    private bool isActive = false;
    private float currentTimer = 0.0f;

    private List<GameObject> objectsInArea = new List<GameObject>();

    public void AddObj(GameObject newObj) { objectsInArea.Add(newObj); }
    public bool GetIsActive() { return isActive; }

    // Start is called before the first frame update
    void Start()
    {
        currentTimer = throwTimer; //How long the orb has in order to hit something (in order to prevent them from staying in scene if nothing is hit)
        activeCollider = activeCol.GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {

        if(isActive == true)
        {
            OrbFunctionality();
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
            //Debug.Log("Orb Successfully Activated. Hit: " + other.gameObject.tag);
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            isActive = true;
            currentTimer = activeTimer; //Resets timer for active time legnth
        }
        else if (other.gameObject.tag == "Player1" || other.gameObject.tag == "Player2")
        {
            //Debug.LogWarning("Ignored Collision. Hit: " + other.gameObject.tag);
            //Do nothing, ignore collision
        }
        else if (isActive == false)
        {
            //Debug.LogWarning("Orb Failed to Activate. Hit: " + other.gameObject.tag);
            parent.hasBeenFired = false; //Resets throw
            Destroy(gameObject);
        }
    }

    private void OrbFunctionality()
    {
        foreach (GameObject obj in objectsInArea)
        {
            if (activeCollider.bounds.Contains(obj.transform.position)) //Obj is in area
            {

                if (currentMode == Mode.PULL)
                {
                    obj.GetComponent<Rigidbody>().AddForce((transform.position - obj.transform.position).normalized * pullPower, ForceMode.Impulse);
                    //Debug.Log(obj.name + " Is in the area.");
                }
                else if (currentMode == Mode.PUSH)
                {
                    Debug.Log("Pushed away: " + obj.name + " at velocity of: " + -(transform.position - obj.transform.position).normalized * pushPower);
                    obj.GetComponent<Rigidbody>().AddForce(-(transform.position - obj.transform.position).normalized * pushPower, ForceMode.Impulse);
                }
            }
            //else //Obj left active area
            //{
            //    objectsInArea.Remove(obj);
            //}
        }
    }
}
