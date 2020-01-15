using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------
// AdolescentIdleState defines what the adolescents will do when they are idling

public class AdolescentIdleState : FSMState
{
    NPCAdolescentController npcAdolescentController;            //NPCAdolescentController script to object
    Health health;                                              //Health script attached to object
        
    //----------------------------------------------------------------------------------------------
    // Constructor
    public AdolescentIdleState(Transform[] wp, NPCAdolescentController npcAdolescent)
    {
        //assign the object's scripts
        npcAdolescentController = npcAdolescent;
        health = npcAdolescent.Health;
        
        //assign speed, waypoints and the stateID
        waypoints = wp;
        stateID = FSMStateID.Idle;
        curRotSpeed = 2.0f;
        curSpeed = 2.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the adolescent when they enter the idle state
    public override void EnterStateInit()
    {
        destPos = waypoints[Random.Range(0, waypoints.Length)].position;
    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the idle state,
    // and what state to transition into
    public override void Reason()
    {
        Transform adolescentTransform = npcAdolescentController.transform;
        Vector3 closestplayer = npcAdolescentController.GetClosestPlayer();

        if (health && health.IsDead())
        {
            npcAdolescentController.PerformTransition(Transition.NoHealth);
        }

        if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.FLEE_DIST))
        {
            if ((health && health.CurrentHealth <= 10) || GameManager.instance.AdolescentCount == 1)
            {
                npcAdolescentController.PerformTransition(Transition.InDanger);
                return;
            }
        }
        if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.CHASE_DIST))
        {
            npcAdolescentController.PerformTransition(Transition.SawPlayer);
            return;
        }
    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the adolescent what to do when it is in the idle state
    public override void Act()
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        npcAdolescentController.NavAgent.CalculatePath(destPos, path);

        if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            destPos = waypoints[Random.Range(0, waypoints.Length)].position;
        }
        else if (Vector3.Distance(npcAdolescentController.transform.position, destPos) < NPCAdolescentController.WAYPOINT_DIST)
        {
            destPos = waypoints[Random.Range(0, waypoints.Length)].position;
        }

        //look towards way point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npcAdolescentController.transform.position);
        npcAdolescentController.transform.rotation = Quaternion.Slerp(npcAdolescentController.transform.rotation, targetRotation,
                                                                curRotSpeed * Time.deltaTime);

        npcAdolescentController.NavAgent.speed = curSpeed;
        npcAdolescentController.NavAgent.SetDestination(destPos);
    }

}
