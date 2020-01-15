using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//---------------------------------------------------------------------------------------------------------
// RuntMeleeAttackState defines what the runts will do when they are attacking the player up close

public class RuntMeleeAttackState : FSMState
{
    NPCRuntController npcRuntController;                            //NPCRuntController script to object
    Health health;                                                  //Health script attached to object
    List<Health> playerHealths = new List<Health>();                //Health script attached to the players
    RuntAttack runtAttack;                                          //RuntAttack script attached to the objects
    
    //----------------------------------------------------------------------------------------------
    // Constructor
    public RuntMeleeAttackState(Transform[] wp, NPCRuntController npcRunt)
    {
        //assign the object's scripts
        npcRuntController = npcRunt;
        health = npcRunt.Health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
        }

        runtAttack = npcRunt.GetComponent<RuntAttack>();

        //assign speed, waypoints, the stateID, and the timers
        waypoints = wp;
        stateID = FSMStateID.MeleeAttacking;
        curRotSpeed = 1.0f;
        curSpeed = 0.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the runt when they enter the melee attack state
    public override void EnterStateInit()
    {

    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the melee attack state,
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

        if ((health && health.CurrentHealth <= 10) || GameManager.instance.RuntCount == 1)
        {
            npcRuntController.PerformTransition(Transition.InDanger);
            return;
        }

        if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.ATTACK_DIST))
        {
            runtAttack.Attacking = true;

        }
        else if (IsInCurrentRange(runtTransform, closestplayer, NPCRuntController.CHASE_DIST))
        {
            //if the player's distance is in range of the chase distance, stop firing and chase 
            //the player
            runtAttack.Attacking = false;
            npcRuntController.PerformTransition(Transition.SawPlayer);
        }
        else
        {
            runtAttack.Attacking = false;
            npcRuntController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the melee attack state
    public override void Act()
    {
        Transform runtTransform = npcRuntController.transform;
        Vector3 closestplayer = npcRuntController.GetClosestPlayer();
        
        Quaternion leftQuatMax = Quaternion.AngleAxis(-45, new Vector3(0, 1, 0));
        Quaternion rightQuatMax = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        // UsefullFunctions.DebugRay(npc.position, leftQuatMax * npc.forward * 5, Color.green);
        // UsefullFunctions.DebugRay(npc.position, rightQuatMax * npc.forward * 5, Color.red);

        Vector3 targetDir = closestplayer - runtTransform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        float angle = Vector3.Angle(targetDir, runtTransform.forward);

        // Rotate the enemy
        runtTransform.rotation = Quaternion.Slerp(runtTransform.rotation, targetRot, Time.deltaTime * curRotSpeed);

        npcRuntController.NavAgent.speed = curSpeed;

    }

}
