using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    Vector3 objectPos;
    public float grabDistance = 10.0f;
    public float pushDistance = 8.0f;
    //public float lastSqrMag;
    //public float sqrMag;

    public GameObject item;
    public GameObject tempParent;
    public bool isHolding = false;
    public bool inPos = false;
    public float AttactSpeed = 7.0f;
    public float throwForce = 3000;

    Color objColor;

    private void Start()
    {
        // reset lastSqrMag
        //lastSqrMag = Mathf.Infinity;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = Camera.main.nearClipPlane;
            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, grabDistance))
            {
                if (raycastHit.transform.tag == "Grabbable Object")
                {
                    isHolding = true;

                    item = raycastHit.collider.gameObject;
                    objColor = item.GetComponent<Renderer>().material.color;
                    item.GetComponent<Rigidbody>().useGravity = false;

                    item.GetComponent<Renderer>().material.color = new Color(objColor.r, objColor.g, objColor.b, 0.5f);
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (item != null)
            {
                isHolding = false;

                objectPos = item.transform.position;
                item.transform.SetParent(null);
                item.GetComponent<Rigidbody>().useGravity = true;
                item.transform.position = objectPos;
                item.GetComponent<Renderer>().material.color = objColor;
            }
        }

        //if (!isHolding && Input.GetMouseButtonDown(1))
        //{
        //    Vector3 pos = Input.mousePosition;
        //    pos.z = Camera.main.nearClipPlane;
        //    Ray ray = Camera.main.ScreenPointToRay(pos);
        //    RaycastHit raycastHit;
        //    if (Physics.SphereCast(transform.position, 1.0f, Vector3.forward, out raycastHit, pushDistance))
        //    {

        //    }
        //}

        //check if holding
        if (isHolding)
        {
            pull();

            if (Input.GetMouseButtonDown(1))
            {
                item.GetComponent<Rigidbody>().AddForce(tempParent.transform.forward * throwForce);
                item.GetComponent<Renderer>().material.color = objColor;
                isHolding = false;
            }
        }
        //else
        //{
        //    if (item != null)
        //    {
        //        objectPos = item.transform.position;
        //        item.transform.SetParent(null);
        //        item.GetComponent<Rigidbody>().useGravity = true;
        //        item.transform.position = objectPos;
        //    }
        //}
    }

    void pull()
    {
        item.transform.SetParent(tempParent.transform);

        //// check the current sqare magnitude
        //sqrMag = (tempParent.transform.position - item.transform.position).sqrMagnitude;
        //// check this against the lastSqrMag
        //// if this is greater than the last,
        //// rigidbody has reached target and is now moving past it
        //if (sqrMag > lastSqrMag)
        //{
        //    // rigidbody has reached target and is now moving past it
        //    // stop the rigidbody by setting the velocity to zero
        //    item.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //    inPos = true;
        //}
        //else if (sqrMag < lastSqrMag)
        //{
        //    inPos = false;
        //}

        ////update the lastSqrMag
        //lastSqrMag = sqrMag;

        if (!checkIfInPos())
        {
            Vector3 dir = (tempParent.transform.position - item.transform.position).normalized * 7.0f;

            item.GetComponent<Rigidbody>().velocity = dir;
        }
        else if (checkIfInPos())
        {
            //item.transform.position = tempParent.transform.position;
            item.GetComponent<Rigidbody>().velocity = Vector3.zero;
            item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }

    bool checkIfInPos()
    {
        bool inPos = false;

        if (item.GetComponent<BoxCollider>().bounds.Contains(tempParent.transform.position))
        {
            inPos = true;
            item.GetComponent<Rigidbody>().velocity = Vector3.zero;
            item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        return inPos;
    }
}