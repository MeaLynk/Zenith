using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------
// RuntChaseState defines what the runts will do when they are in chase mode

public class RuntChaseState : FSMState
{
    NPCRuntController npcRuntController;       //NPCRuntController script to object
    Health health;                             //Health script attached to object
    List<Health> playerHealths;                //Health script attached to the players
    SlotManager closestPlayerSlots;            //SlotManager script attached to the closest player

    //timers intended to track when the runt controller should update its state and variables
    float elapsedTime;
    float intervalTime;

    //current playerSlot index
    int availSlotIndex;

    //the distance from the player to the runt
    List<float> playerDistances = new List<float>();

    //----------------------------------------------------------------------------------------------
    // Constructor
    public RuntChaseState(Transform[] wp, NPCRuntController npcRunt)
    {
        //assign the object's scripts
        npcRuntController = npcRunt;
        health = npcRunt.health;
        playerHealths = new List<Health>();

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
            playerDistances.Add(Vector3.Distance(npcRunt.transform.position, GameManager.instance.Players[i].transform.position));
        }

        //assign speed, waypoints, the stateID, and the timers
        waypoints = wp;
        stateID = FSMStateID.Chasing;
        elapsedTime = 0.0f;
        intervalTime = 1.0f;
        availSlotIndex = -1;
        curRotSpeed = 2.0f;
        curSpeed = 3.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the runt when they enter the chase state
    public override void EnterStateInit()
    {
        // get a slot position
        int closerPlayer = -1;
        float distance = float.PositiveInfinity;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            if (playerDistances[i] < distance)
            {
                distance = playerDistances[i];
                closerPlayer = i;
            }
        }

        closestPlayerSlots = npcRuntController.GetPlayerSlotManager(closerPlayer);
        closestPlayerSlots.ClearSlots(npcRuntController.gameObject);
        availSlotIndex = closestPlayerSlots.ReserveSlotAroundObject(npcRuntController.gameObject);

        if (availSlotIndex != -1)
        {
            // if the available slot index isn't a non-existent number, assign it to the destPos
            destPos = closestPlayerSlots.GetSlotPosition(availSlotIndex);
        }
        else
        {
            //otherwise, assign the destPos to be the player's position
            destPos = npcRuntController.GetPlayerTransform(closerPlayer).position;
        }

        elapsedTime = 0.0f;
    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the chase state,
    // and what state to transition into
    public override void Reason()
    {
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestPlayerPos;

        int closestPlayerIndex = 0;
        float closestPlayerDist = float.PositiveInfinity;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerDistances[i] = Vector3.Distance(runtTransform.position, npcRuntController.GetPlayerTransform(i).position);
            if (playerDistances[i] < closestPlayerDist)
            {
                closestPlayerDist = playerDistances[i];
                closestPlayerIndex = i;
            }
        }

        closestPlayerPos = npcRuntController.GetPlayerTransform(closestPlayerIndex).position;
        closestPlayerSlots = npcRuntController.GetPlayerSlotManager(closestPlayerIndex);

        if (health && health.IsDead())
        {
            //if the health script is present on the runt and its dead, transition to the dead state
            npcRuntController.PerformTransition(Transition.NoHealth);
            return;
        }

        bool playersDead = true;
        for (int i = 0; i < 2; i++)
        {
            if (playerHealths[i] && !playerHealths[i].IsDead())
            {
                playersDead = false;
                break;
            }
        }
        if (playersDead)
        {
            npcRuntController.PerformTransition(Transition.PlayerDead);
            return;
        }

        if (IsInCurrentRange(runtTransform, closestPlayerPos, NPCRuntController.FLEE_DIST))
        {
            if ((health && health.CurrentHealth <= 10) || GameManager.instance.RuntCount == 1)
            {
                npcRuntController.PerformTransition(Transition.InDanger);
                return;
            }
        }

        // Keep track of the player slots and release and grab new one every so often...
        elapsedTime += Time.deltaTime;
        if (elapsedTime > intervalTime)
        {
            elapsedTime = 0.0f;
            closestPlayerSlots.ReleaseSlot(availSlotIndex);
            availSlotIndex = closestPlayerSlots.ReserveSlotAroundObject(npcRuntController.gameObject);
            if (availSlotIndex != -1)
            {
                destPos = closestPlayerSlots.GetSlotPosition(availSlotIndex);
            }
            else
            {
                destPos = npcRuntController.GetPlayerTransform(closestPlayerIndex).position;
            }
        }

        if (IsInCurrentRange(runtTransform, destPos, NPCRuntController.CHASE_DIST))
        {
            // want to check if we are close to our destination position and then transition to attack
            if (IsInCurrentRange(runtTransform, destPos, NPCRuntController.SLOT_DIST))
            {
                npcRuntController.PerformTransition(Transition.ReachPlayer);
            }

        }
        else
        {
            //lost player
            npcRuntController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the chase state
    public override void Act()
    {
        //look towards slot position
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npcRuntController.transform.position);
        npcRuntController.transform.rotation = Quaternion.Slerp(npcRuntController.transform.rotation, targetRotation,
                                                                curRotSpeed * Time.deltaTime);

        npcRuntController.navAgent.speed = curSpeed;
        npcRuntController.navAgent.SetDestination(destPos);
    }

}
