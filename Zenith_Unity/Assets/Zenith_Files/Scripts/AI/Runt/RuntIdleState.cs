using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------
// RuntIdleState defines what the runts will do when they are idling

public class RuntIdleState : FSMState
{
    NPCRuntController npcRuntController;            //NPCRuntController script to object
    Health health;                                  //Health script attached to object

    //the distance from the player to the runt
    List<float> playerDistances = new List<float>();
    
    public RuntIdleState(Transform[] wp, NPCRuntController npcRunt)
    {
        //assign the object's scripts
        npcRuntController = npcRunt;
        health = npcRunt.health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerDistances.Add(Vector3.Distance(npcRunt.transform.position, GameManager.instance.Players[i].transform.position));
        }

        //assign speed, waypoints and the stateID
        waypoints = wp;
        stateID = FSMStateID.Idle;
        curRotSpeed = 2.0f;
        curSpeed = 2.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the runt when they enter the idle state
    public override void EnterStateInit()
    {
        destPos = waypoints[Random.Range(0, waypoints.Length)].position;
    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the idle state,
    // and what state to transition into
    public override void Reason()
    {
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestplayer;

        int closerPlayer = 0;
        float distance = float.PositiveInfinity;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerDistances[i] = Vector3.Distance(runtTransform.position, npcRuntController.GetPlayerTransform(i).position);
            if (playerDistances[i] < distance)
            {
                distance = playerDistances[i];
                closerPlayer = i;
            }
        }

        closestplayer = npcRuntController.GetPlayerTransform(closerPlayer).position;

        if (health && health.IsDead())
        {
            npcRuntController.PerformTransition(Transition.NoHealth);
        }

        if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.FLEE_DIST))
        {
            if ((health && health.CurrentHealth <= 10) || GameManager.instance.RuntCount == 1)
            {
                npcRuntController.PerformTransition(Transition.InDanger);
                return;
            }
        }
        if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.CHASE_DIST))
        {
            npcRuntController.PerformTransition(Transition.SawPlayer);
            return;
        }
    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the idle state
    public override void Act()
    {

        if (Vector3.Distance(npcRuntController.transform.position, destPos) < NPCRuntController.SLOT_DIST)
        {
            Vector3 newDestPos = waypoints[Random.Range(0, waypoints.Length)].position;
            destPos = newDestPos;
        }

        //look towards way point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npcRuntController.transform.position);
        npcRuntController.transform.rotation = Quaternion.Slerp(npcRuntController.transform.rotation, targetRotation,
                                                                curRotSpeed * Time.deltaTime);

        npcRuntController.navAgent.speed = curSpeed;
        npcRuntController.navAgent.SetDestination(destPos);
    }

}
