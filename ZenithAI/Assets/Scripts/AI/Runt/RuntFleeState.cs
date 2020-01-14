﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------
// RuntFleeState defines what the runts will do when they are hiding from the player

public class RuntFleeState : FSMState
{
    NPCRuntController npcRuntController;                            //NPCRuntController script to object
    Health health;                                                  //Health script attached to object
    List<Health> playerHealths = new List<Health>();                //Health script attached to the players

    //the distance from the player to the runt
    List<float> playerDistances = new List<float>();

    //----------------------------------------------------------------------------------------------
    // Constructor
    public RuntFleeState(Transform[] wp, NPCRuntController npcRunt)
    {
        //assign the object's scripts
        npcRuntController = npcRunt;
        health = npcRunt.health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
            playerDistances.Add(Vector3.Distance(npcRunt.transform.position, GameManager.instance.Players[i].transform.position));
        }

        //assign speed, waypoints, and the stateID
        waypoints = wp;
        stateID = FSMStateID.Fleeing;
        curRotSpeed = 2.0f;
        curSpeed = 6.0f;

    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the runt when they enter the flee state
    public override void EnterStateInit()
    {
        //get the waypoint's position that is farthest from the player and sets it 
        //as the destination position
        int closerPlayer = -1;
        float playerdistance = float.PositiveInfinity;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerDistances[i] = Vector3.Distance(npcRuntController.transform.position, npcRuntController.GetPlayerTransform(i).position);
            if (playerDistances[i] < playerdistance)
            {
                playerdistance = playerDistances[i];
                closerPlayer = i;
            }
        }

        float distance = 0.0f;
        Vector3 hidePos = Vector3.zero;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (Vector3.Distance(npcRuntController.GetPlayerTransform(closerPlayer).position, waypoints[i].transform.position) > distance)
            {
                distance = Vector3.Distance(npcRuntController.GetPlayerTransform(closerPlayer).position, waypoints[i].transform.position);
                hidePos = waypoints[i].position;
            }
        }

        destPos = hidePos;
    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the flee state,
    // and what state to transition into
    public override void Reason()
    {
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestplayer;

        int closerPlayer = -1;
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
            return;
        }
        bool playersDead = true;
        for (int i = 0; i < GameManager.instance.Players.Count; i++)
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

        if (!IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.FLEE_DIST)
            && IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.RANGED_ATTACK_DIST))
        {
            npcRuntController.PerformTransition(Transition.ReachPlayer);
            return;
        }
        else if (!IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.FLEE_DIST)
            && !IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.RANGED_ATTACK_DIST))
        {
            npcRuntController.PerformTransition(Transition.LostPlayer);
            return;
        }
    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the flee state
    public override void Act()
    {
        if (IsInCurrentRange(npcRuntController.transform, destPos, NPCRuntController.SLOT_DIST))
        {
            Vector3 closestplayer;

            int closerPlayer = -1;
            float playerDistance = float.PositiveInfinity;

            for (int i = 0; i < GameManager.instance.Players.Count; i++)
            {
                if (playerDistances[i] < playerDistance)
                {
                    playerDistance = playerDistances[i];
                    closerPlayer = i;
                }
            }

            closestplayer = npcRuntController.GetPlayerTransform(closerPlayer).position;

            float distance = 0.0f;
            Vector3 hidePos = Vector3.zero;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (Vector3.Distance(closestplayer, waypoints[i].transform.position) > distance)
                {
                    distance = Vector3.Distance(closestplayer, waypoints[i].transform.position);
                    hidePos = waypoints[i].position;
                }
            }

            destPos = hidePos;
        }

        npcRuntController.navAgent.speed = curSpeed;
        npcRuntController.navAgent.SetDestination(destPos);
    }

}