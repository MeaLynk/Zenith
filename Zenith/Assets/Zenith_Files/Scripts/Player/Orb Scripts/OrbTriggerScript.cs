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
        if (other.tag == "Grabbable Object" || other.tag == "Runt" || other.tag == "Player1" || other.tag == "Player2")
        {
            parent.AddObj(other.gameObject);
            Debug.Log("Added new obj for push/pull");
        }
    }
}
