using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pickup : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject playerGuide;
    public GameObject grabbedItem;
    public Slider AttractStamina;
    public Slider RepulseStamina;

    public bool isHolding = false;
    public bool inPos = false;

    public float grabDistance = 10.0f;
    public float pushDistance = 8.0f;
    public float AttactSpeed = 7.0f;
    public float throwForce = 3000;
    public float pushForce = 150;
    //public float lastSqrMag;
    //public float sqrMag;

    Color objColor;
    private Rigidbody objRigid;
    private Renderer objRender;
    private Collider objCollider;
    private float objectMass = 0;
    private float aElapsedTime = 0.0f;
    private float rElapsedTime = 0.0f;
    private Color aColor;
    private Color rColor;


    private void Start()
    {
        // reset lastSqrMag
        //lastSqrMag = Mathf.Infinity;
        Image[] images;
        images = AttractStamina.gameObject.GetComponentsInChildren<Image>();
        aColor = images[1].color;

        images = RepulseStamina.gameObject.GetComponentsInChildren<Image>();
        rColor = images[1].color;
    }

    // Update is called once per frame
    void Update()
    {
        if (AttractStamina.value < AttractStamina.maxValue && !isHolding)
        {
            aElapsedTime += Time.deltaTime;
        }
        if (RepulseStamina.value < RepulseStamina.maxValue)
        {
            rElapsedTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!isHolding)
            {
                Vector3 pos = Input.mousePosition;
                pos.z = playerCamera.nearClipPlane;
                Ray ray = playerCamera.ScreenPointToRay(pos);
                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit, grabDistance))
                {
                    if (raycastHit.transform.tag == "Grabbable Object")
                    {
                        grabbedItem = raycastHit.collider.gameObject;
                        objRigid = grabbedItem.GetComponent<Rigidbody>();
                        objectMass = objRigid.mass;
                        objRigid.mass = 1;

                        if (AttractStamina.value - objectMass >= 0)
                        {
                            StopCoroutine(refilStam(true));
                            objRender = grabbedItem.GetComponent<Renderer>();
                            objCollider = grabbedItem.GetComponent<Collider>();

                            objColor = objRender.material.color;
                            objRigid.useGravity = false;
                            objRender.material.color = new Color(objColor.r, objColor.g, objColor.b, 0.5f);

                            useStamina(true, objectMass);

                            isHolding = true;

                            aElapsedTime = 0;
                        }
                        else
                        {
                            objRigid.mass = objectMass;
                            grabbedItem = null;
                            objRigid = null;
                            objectMass = 0;

                            StartCoroutine(flashing(true));
                        }
                    }
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (grabbedItem != null)
            {
                isHolding = false;

                grabbedItem.transform.SetParent(null);
                objRigid.velocity = Vector3.zero;
                objRigid.angularVelocity = Vector3.zero;
                objRigid.useGravity = true;
                objRender.material.color = objColor;
                objRigid.mass = objectMass;
                objectMass = 0;
                grabbedItem = null;
                objRigid = null;
                objRender = null;
                objCollider = null;
                objColor = Color.white;
            }
        }

        if (!isHolding && Input.GetMouseButtonDown(1))
        {
            if (RepulseStamina.value - 20 >= 0)
            {
                StopCoroutine(refilStam(false));
                useStamina(false, 20);

                RaycastHit[] raycastHit;

                raycastHit = Physics.SphereCastAll(playerCamera.transform.position, 1.5f, playerCamera.transform.forward, pushDistance);

                for (int i = 0; i < raycastHit.Length; i++)
                {
                    if (raycastHit[i].collider.tag == "Grabbable Object")
                    {
                        Vector3 dir = (raycastHit[i].transform.position - playerGuide.transform.position).normalized * pushForce;

                        raycastHit[i].collider.gameObject.GetComponent<Rigidbody>().velocity = dir;
                    }
                }

                rElapsedTime = 0;
            }
            else
            {
                StartCoroutine(flashing(false));
            }
        }

        //check if holding
        if (isHolding)
        {
            pull();

            if (Input.GetMouseButtonDown(1))
            {
                if (RepulseStamina.value - objectMass >= 0)
                {
                    StopCoroutine(refilStam(false));
                    useStamina(false, objectMass);

                    grabbedItem.transform.SetParent(null);
                    objRigid.AddForce(playerGuide.transform.forward * throwForce * objRigid.mass);
                    objRigid.useGravity = true;
                    objRender.material.color = objColor;
                    objRigid.mass = objectMass;
                    objectMass = 0;
                    isHolding = false;
                    grabbedItem = null;
                    objRigid = null;
                    objRender = null;
                    objCollider = null;
                    objColor = Color.white;
                    rElapsedTime = 0;
                }
                else
                {
                    StartCoroutine(flashing(false));
                }
            }
        }

        if(!isHolding && aElapsedTime >= 2)
        {
            StartCoroutine(refilStam(true));
            if(AttractStamina.value == AttractStamina.maxValue)
            {
                aElapsedTime = 0;
                StopCoroutine(refilStam(true));
            }
        }
        if(rElapsedTime >= 2)
        {
            StartCoroutine(refilStam(false));
            if (RepulseStamina.value == RepulseStamina.maxValue)
            {
                rElapsedTime = 0;
                StopCoroutine(refilStam(false));
            }
        }
    }

    void pull()
    {
        grabbedItem.transform.SetParent(playerGuide.transform);

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
            Vector3 dir = (playerGuide.transform.position - grabbedItem.transform.position).normalized * AttactSpeed;

            objRigid.velocity = dir;
        }
        else if (checkIfInPos())
        {
            objRigid.velocity = Vector3.zero;
            objRigid.angularVelocity = Vector3.zero;
        }
    }

    bool checkIfInPos()
    {
        bool inPos = false;

        if (objCollider.bounds.Contains(playerGuide.transform.position))
        {
            inPos = true;
            objRigid.velocity = Vector3.zero;
            objRigid.angularVelocity = Vector3.zero;
        }

        return inPos;
    }

    void useStamina(bool attractRepulse, float staminaUse) //Attract = True, Repulse = False
    {
        if(attractRepulse)
        {
            AttractStamina.value -= staminaUse;
        }
        else
        {
            RepulseStamina.value -= staminaUse;
        }
    }

    IEnumerator flashing(bool attractRepulse) //Attract = True, Repulse = False
    {
        Image[] images;
        int flashes = 0;
        Color color;

        if (attractRepulse)
        {
            images = AttractStamina.gameObject.GetComponentsInChildren<Image>();
            color = aColor;
        }

        else
        {
            images = RepulseStamina.gameObject.GetComponentsInChildren<Image>();
            color = rColor;
        }

        while (flashes < 3)
        {
            for (int i = 1; i < images.Length; i++)
            {
                images[i].color = Color.red;
            }
            yield return new WaitForSeconds(0.2f);
            for (int i = 1; i < images.Length; i++)
            {
                images[i].color = color;
            }
            yield return new WaitForSeconds(0.2f);
            flashes++;
        }
    }

    IEnumerator refilStam(bool attractRepulse) //Attract = True, Repulse = False
    {
        if(attractRepulse)
        {
            while(AttractStamina.value < AttractStamina.maxValue)
            {
                AttractStamina.value += 5;
                if(AttractStamina.value > AttractStamina.maxValue)
                {
                    AttractStamina.value = AttractStamina.maxValue;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            while (RepulseStamina.value < RepulseStamina.maxValue)
            {
                RepulseStamina.value += 5;
                if (RepulseStamina.value > RepulseStamina.maxValue)
                {
                    RepulseStamina.value = RepulseStamina.maxValue;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}