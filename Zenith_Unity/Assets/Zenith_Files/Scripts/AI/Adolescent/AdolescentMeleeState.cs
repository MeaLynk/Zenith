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
    AdolescentAttack adolescentAttack;                              //AdolescenetAttack script attached to the objects

    //----------------------------------------------------------------------------------------------
    // Constructor
    public AdolescentMeleeAttackState(Transform[] wp, NPCAdolescentController npcAdolescent)
    {
        //assign the object's scripts
        npcAdolescentController = npcAdolescent;
        health = npcAdolescent.Health;

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
        }

        adolescentAttack = npcAdolescentController.GetComponent<AdolescentAttack>();

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
        Vector3 closestplayer = npcAdolescentController.GetClosestPlayer();

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
            adolescentAttack.Attacking = true;
        }
        else if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.CHASE_DIST))
        {
            //if the player's distance is in range of the chase distance, stop firing and chase 
            //the player
            adolescentAttack.Attacking = false;
            npcAdolescentController.PerformTransition(Transition.SawPlayer);
        }
        else
        {
            adolescentAttack.Attacking = false;
            npcAdolescentController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the runt what to do when it is in the ranged attack state
    public override void Act()
    {
        //adjust the runts's rotation if need be, fire 
        Transform adolescentTransform = npcAdolescentController.transform;
        Vector3 closestplayer = npcAdolescentController.GetClosestPlayer();

        Quaternion leftQuatMax = Quaternion.AngleAxis(-45, new Vector3(0, 1, 0));
        Quaternion rightQuatMax = Quaternion.AngleAxis(45, new Vector3(0, 1, 0));

        // UsefullFunctions.DebugRay(npc.position, leftQuatMax * npc.forward * 5, Color.green);
        // UsefullFunctions.DebugRay(npc.position, rightQuatMax * npc.forward * 5, Color.red);

        Vector3 targetDir = closestplayer - adolescentTransform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        // Rotate the enemy
        adolescentTransform.rotation = Quaternion.Slerp(adolescentTransform.rotation, targetRot, Time.deltaTime * curRotSpeed);

        npcAdolescentController.NavAgent.speed = curSpeed;

    }

}
