﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//---------------------------------------------------------------------------------------------------------
// RuntRangedAttackState defines what the runts will do when they are attacking the player from a distance
public class RuntRangedAttackState : FSMState
{
    NPCRuntController npcRuntController;                            //NPCRuntController script to object
    Health health;                                                  //Health script attached to object
    List<Health> playerHealths = new List<Health>();                //Health script attached to the players

    //EnemyTankShooting enemyTankShooting;            //EnemyTankShooting script attached tothe objects
        
    //----------------------------------------------------------------------------------------------
    // Constructor
    public RuntRangedAttackState(Transform[] wp, NPCRuntController npcRunt)
    {
        //assign the object's scripts
        npcRuntController = npcRunt;
        health = npcRuntController.Health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
        }

        //enemyTankShooting = npcRuntController.GetComponent<EnemyTankShooting>();

        //assign speed, waypoints, the stateID, and the timers
        waypoints = wp;
        stateID = FSMStateID.RangedAttacking;
        curRotSpeed = 2.0f;
        curSpeed = 0.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the runt when they enter the ranged attack state
    public override void EnterStateInit()
    {

    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the ranged attack state,
    // and what state to transition into
    public override void Reason()
    {
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestplayer = npcRuntController.GetClosestPlayer();
        
        if (health && health.IsDead())
        {
            //if the health script is present on the runt and its dead, transition to the dead
            //state
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

        if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.FLEE_DIST))
        {
            if (health && health.CurrentHealth <= 10 || GameManager.instance.RuntCount == 1)
            {
                //if the health script is present on the runt and its health is below 10,
                //transition to the hide state
                npcRuntController.PerformTransition(Transition.InDanger);
                return;
            }
        }
        if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.RANGED_ATTACK_DIST)
            && !IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.FLEE_DIST))
        {
            // wait to shoot
            //if (npcTankController.receivedAttackCommand)
            //enemyTankShooting.Firing = true;
            //else
            //enemyTankShooting.Firing = false;
        }
        else
        {
            //enemyTankShooting.Firing = false;
            npcRuntController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the ranged attack state
    public override void Act()
    {
        //adjust the runts's rotation and fire 
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestplayer = npcRuntController.GetClosestPlayer();

        Quaternion leftQuatMax = Quaternion.AngleAxis(-45, new Vector3(0, 1, 0));
        Quaternion rightQuatMax = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        // UsefullFunctions.DebugRay(npc.position, leftQuatMax * npc.forward * 5, Color.green);
        // UsefullFunctions.DebugRay(npc.position, rightQuatMax * npc.forward * 5, Color.red);

        Vector3 targetDir = closestplayer - runtTransform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        // Rotate the enemy
        runtTransform.rotation = Quaternion.Slerp(runtTransform.rotation, targetRot, Time.deltaTime * curRotSpeed);

        npcRuntController.NavAgent.speed = curSpeed;

    }

}
