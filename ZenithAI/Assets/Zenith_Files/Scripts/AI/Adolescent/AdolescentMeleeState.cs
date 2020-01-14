using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//---------------------------------------------------------------------------------------------------------
// AdolescentMeleeAttackState defines what the adolescents will do when they are attacking the player up close

public class AdolescentMeleeAttackState : FSMState
{
    NPCAdolescentController npcAdolescentController;                //NPCAdolescentController script to object
    Health health;                                                  //Health script attached to object
    List<Health> playerHealths = new List<Health>();                //Health script attached to the players
    //EnemyTankShooting enemyTankShooting;                          //EnemyTankShooting script attached to the objects

    //the distance from the player to the adolescent
    List<float> playerDistances = new List<float>();
    
    //----------------------------------------------------------------------------------------------
    // Constructor
    public AdolescentMeleeAttackState(Transform[] wp, NPCAdolescentController npcAdolescent)
    {
        //assign the object's scripts
        npcAdolescentController = npcAdolescent;
        health = npcAdolescent.health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
            playerDistances.Add(Vector3.Distance(npcAdolescent.transform.position, GameManager.instance.Players[i].transform.position));
        }

        //enemyTankShooting = npcAdolescentController.GetComponent<EnemyTankShooting>();

        //assign speed, waypoints, the stateID, and the timers
        waypoints = wp;
        stateID = FSMStateID.MeleeAttacking;
        curRotSpeed = 1.0f;
        curSpeed = 0.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the adolescent when they enter the melee attack state
    public override void EnterStateInit()
    {

    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the melee attack state,
    // and what state to transition into
    public override void Reason()
    {
        Transform adolescentTransform = npcAdolescentController.transform;
        Vector3 closestplayer;

        int closerPlayer = -1;
        float distance = float.PositiveInfinity;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerDistances[i] = Vector3.Distance(adolescentTransform.position, npcAdolescentController.GetPlayerTransform(i).position);
            if (playerDistances[i] < distance)
            {
                distance = playerDistances[i];
                closerPlayer = i;
            }
        }

        closestplayer = npcAdolescentController.GetPlayerTransform(closerPlayer).position;

        if (health && health.IsDead())
        {
            //if the health script is present on the adolescent and its dead, transition to the dead
            //state
            npcAdolescentController.PerformTransition(Transition.NoHealth);
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
            npcAdolescentController.PerformTransition(Transition.PlayerDead);
            return;
        }

        if ((health && health.CurrentHealth <= 10) || GameManager.instance.AdolescentCount == 1)
        {
            npcAdolescentController.PerformTransition(Transition.InDanger);
            return;
        }

        if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.ATTACK_DIST))
        {
            // wait to shoot
            //if (npcTankController.receivedAttackCommand)
            //   enemyTankShooting.Firing = true;
            //else
            // enemyTankShooting.Firing = false;

        }
        else if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.CHASE_DIST))
        {
            //if the player's distance is in range of the chase distance, stop firing and chase 
            //the player
            // enemyTankShooting.Firing = false;
            npcAdolescentController.PerformTransition(Transition.SawPlayer);
        }
        else
        {
            //enemyTankShooting.Firing = false;
            npcAdolescentController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the ranged attack state
    public override void Act()
    {
        //adjust the runts's rotation if need be, fire 

        Transform npc = npcAdolescentController.transform;
        Vector3 closestplayer;

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

        closestplayer = npcAdolescentController.GetPlayerTransform(closerPlayer).position;

        Quaternion leftQuatMax = Quaternion.AngleAxis(-45, new Vector3(0, 1, 0));
        Quaternion rightQuatMax = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        // UsefullFunctions.DebugRay(npc.position, leftQuatMax * npc.forward * 5, Color.green);
        // UsefullFunctions.DebugRay(npc.position, rightQuatMax * npc.forward * 5, Color.red);

        Vector3 targetDir = closestplayer - npc.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        float angle = Vector3.Angle(targetDir, npc.forward);

        // Rotate the enemy
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRot, Time.deltaTime * curRotSpeed);

        npcAdolescentController.navAgent.speed = curSpeed;

    }

}
