using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------------------------------------------------
// AdolescentChargeState defines what the adolescents will do when they are in charge mode

public class AdolescentChargeState : FSMState
{
    NPCAdolescentController npcAdolescentController;        //NPCAdolescentController script to object
    Health health;                                          //Health script attached to object
    List<Health> playerHealths;                             //Health script attached to the players
    SlotManager closestPlayerSlots;                         //SlotManager script attached to the closest player

    //timers intended to track when the adolescent controller should update its state and variables
    float elapsedTime;
    float intervalTime;

    //current playerSlot index
    int availSlotIndex;
    
    //----------------------------------------------------------------------------------------------
    // Constructor
    public AdolescentChargeState(Transform[] wp, NPCAdolescentController npcAdolescent)
    {
        //assign the object's scripts
        npcAdolescentController = npcAdolescent;
        health = npcAdolescent.Health;
        playerHealths = new List<Health>();

        for (int i = 0; i < GameManager.instance.Players.Count; i++)
        {
            playerHealths.Add(GameManager.instance.Players[i].GetComponent<Health>());
        }

        //assign speed, waypoints, the stateID, and the timers
        waypoints = wp;
        stateID = FSMStateID.Charging;
        elapsedTime = 0.0f;
        intervalTime = 1.0f;
        availSlotIndex = -1;
        curRotSpeed = 2.0f;
        curSpeed = 3.0f;
    }

    //----------------------------------------------------------------------------------------------
    // EnterStateInit() is used to initialize the adolescent when they enter the charge state
    public override void EnterStateInit()
    {
        // get a slot position
        Vector3 closestplayer = npcAdolescentController.GetClosestPlayer();

        closestPlayerSlots = npcAdolescentController.GetPlayerSlotManager(npcAdolescentController.PlayerTarget);
        closestPlayerSlots.ClearSlots(npcAdolescentController.gameObject);
        availSlotIndex = closestPlayerSlots.ReserveSlotAroundObject(npcAdolescentController.gameObject);

        if (availSlotIndex != -1)
        {
            // if the available slot index isn't a non-existent number, assign it to the destPos
            destPos = closestPlayerSlots.GetSlotPosition(availSlotIndex);
        }
        else
        {
            //otherwise, assign the destPos to be the player's position
            destPos = closestplayer;
        }

        elapsedTime = 0.0f;
    }

    //----------------------------------------------------------------------------------------------
    // Reason() is used to determine the conditions to transition out of the charge state,
    // and what state to transition into
    public override void Reason()
    {
        Transform adolescentTransform = npcAdolescentController.transform;
        Vector3 closestplayer = npcAdolescentController.GetClosestPlayer();

        closestPlayerSlots = npcAdolescentController.GetPlayerSlotManager(npcAdolescentController.PlayerTarget);

        if (health && health.IsDead())
        {
            //if the health script is present on the adolescent and its dead, transition to the dead state
            npcAdolescentController.PerformTransition(Transition.NoHealth);
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
            npcAdolescentController.PerformTransition(Transition.PlayerDead);
            return;
        }

        if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.FLEE_DIST))
        {
            if ((health && health.CurrentHealth <= 10) || GameManager.instance.AdolescentCount == 1)
            {
                npcAdolescentController.PerformTransition(Transition.InDanger);
                return;
            }
        }

        // Keep track of the player slots and release and grab new one every so often...
        elapsedTime += Time.deltaTime;
        if (elapsedTime > intervalTime)
        {
            elapsedTime = 0.0f;
            closestPlayerSlots.ReleaseSlot(availSlotIndex);
            availSlotIndex = closestPlayerSlots.ReserveSlotAroundObject(npcAdolescentController.gameObject);
            if (availSlotIndex != -1)
            {
                destPos = closestPlayerSlots.GetSlotPosition(availSlotIndex);
            }
            else
            {
                destPos = closestplayer;
            }
        }

        if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.CHASE_DIST))
        {
            // want to check if we are close to our destination position and then transition to attack
            if (IsInCurrentRange(adolescentTransform, closestplayer, NPCAdolescentController.SLOT_DIST))
            {
                npcAdolescentController.PerformTransition(Transition.ReachPlayer);
            }

        }
        else
        {
            //lost player
            npcAdolescentController.PerformTransition(Transition.LostPlayer);
        }

    }

    //----------------------------------------------------------------------------------------------
    // Act() is used to tell the adolescent what to do when it is in the charge state
    public override void Act()
    {
        //look towards slot position
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npcAdolescentController.transform.position);
        npcAdolescentController.transform.rotation = Quaternion.Slerp(npcAdolescentController.transform.rotation, targetRotation,
                                                                curRotSpeed * Time.deltaTime);

        npcAdolescentController.NavAgent.speed = curSpeed;
        npcAdolescentController.NavAgent.SetDestination(destPos);
    }

}
