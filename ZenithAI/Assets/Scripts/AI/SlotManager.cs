using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{
    private Color[] slotColours = new Color[] { Color.green, Color.blue, Color.red, Color.yellow, Color.white, Color.black };
    //one slot contains the game object occpying the slot
    private List<GameObject> slotsAroundObject;

    public int maxNumOfSlots = 6;
    public float distance = 4.0f;

    private void Awake()
    {
        slotsAroundObject = new List<GameObject>();
        for (int i = 0; i < maxNumOfSlots; i++)
        {
            slotsAroundObject.Add(null);
        }
    }
    // Use this for initialization
    void Start()
    {


    }

    //Get the slot position (use position in list based on maxNumOfSlots to form a circle around object)
    public Vector3 GetSlotPosition(int index)
    {
        float degreesPerIndex = 360f / maxNumOfSlots;
        Vector3 currPos = transform.position;
        Quaternion rotationAboutAxis = Quaternion.AngleAxis(degreesPerIndex * index, new Vector3(0, 1, 0));
        Vector3 slotPosition = currPos + (rotationAboutAxis * transform.forward * distance);

        return slotPosition;
    }


    //Get(Reserve) the nearest slot to you(attacker) that is not already occupied
    //returns index of the available slot or -1 if there are no available slots
    public int ReserveSlotAroundObject(GameObject attacker)
    {
        if (attacker == null) return -1;

        int slotIndex = -1; // - 1 indicates that there are no slots available

        //get the object's current position
        Vector3 objectCurrentPos = transform.position;

        //get the offset position based of the direction to the attacker
        Vector3 offSet = (attacker.transform.position - objectCurrentPos).normalized * distance;

        //add the offset position to the objects position (this is where you (attacker want to go)
        Vector3 bestPosition = objectCurrentPos + offSet;

        float bestDistance = Mathf.Infinity;
        //find the closest slot position to the bestPosition (where you want to go)
        for (int index = 0; index < slotsAroundObject.Count; index++)
        {
            //only use the slots that are not occupied -- they are considered free/available
            if (slotsAroundObject[index] == null)
            {
                //check to see if slot is not in obstacle
                if (GridManager.instance.IsInObstacle(GetSlotPosition(index)) == false &&
                    GridManager.instance.IsInBounds(GetSlotPosition(index)))
                {//get distance of the bestPosition to the current slot
                    float distance = Vector3.Distance(GetSlotPosition(index), bestPosition);
                    if (distance < bestDistance)
                    {
                        //found a new distance
                        bestDistance = distance;
                        slotIndex = index;
                    }
                }
            }
        }
        if (slotIndex != -1)
        {
            slotsAroundObject[slotIndex] = attacker;
        }
        return slotIndex;
    }

    public int FindNumberOfEmptySlots()
    {
        int numSlots = 0;
        for (int index = 0; index < slotsAroundObject.Count; index++)
        {
            if (slotsAroundObject[index] != null)
            {
                numSlots++;
            }
        }
        return numSlots;
    }

    public void ReleaseSlot(int index)
    {
        if (index >= 0 && index < slotsAroundObject.Count)
        {
            slotsAroundObject[index] = null;
        }

    }

    public void ClearSlots(GameObject go)
    {
        for (int i = 0; i < slotsAroundObject.Count; i++)
        {
            if (slotsAroundObject[i] == go)
            {
                slotsAroundObject[i] = null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DebugDrawSlots()
    {
        if (slotsAroundObject == null) return;

        Color c = Color.green;
        for (int index = 0; index < slotsAroundObject.Count; index++)
        {
            if (slotsAroundObject[index] == null)
            {
                c = Color.black;
            }
            else
            {
                int colourIndex = index;
                if (index >= slotColours.Length)
                {
                    colourIndex = colourIndex % slotColours.Length;
                }
                c = slotColours[colourIndex];

            }

            Gizmos.color = c;
            Gizmos.DrawWireSphere(GetSlotPosition(index), 0.5f);
            //UsefullFunctions.DebugRay(transform.position, ((GetSlotPosition(index) - transform.position).normalized) * distance, c);
            //UsefullFunctions.DebugRay(transform.position, transform.forward * 5.0f, Color.cyan);
        }
    }

    void OnDrawGizmosSelected()
    {
        DebugDrawSlots();
    }
}