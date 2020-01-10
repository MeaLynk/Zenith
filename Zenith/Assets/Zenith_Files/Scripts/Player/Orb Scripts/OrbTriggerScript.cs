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
        if (other.tag == "Grabbable Object" || other.tag == "Runt")
        {
            parent.AddObj(other.gameObject);
        }
    }
}
