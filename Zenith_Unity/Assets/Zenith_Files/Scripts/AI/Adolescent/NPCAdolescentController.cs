﻿//NPCAdolescentController
//This class is derived from AdvancedFSM and holds the FSM for the NPC Adolescent
//each Adolescent must have this attached to it in order to have "Adolescent" behaviour
//
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NPCAdolescentController : AdvancedFSM
{
    public static int SLOT_DIST = 2;
    public static int WAYPOINT_DIST = 5;
    public static int ATTACK_DIST = 3;
    public static int CHASE_DIST = 10;
    public static int FLEE_DIST = 6;

    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public Health health;
    [HideInInspector] public Rigidbody rigBody;
    [HideInInspector] public bool receivedAttackCommand = false;
    [HideInInspector] public Transform[] pointList;
    [HideInInspector] public Transform[] fleePoints;


    public Transform GetPlayerTransform(int index)
    {
        Transform playerTransform = GameManager.instance.Players[index].transform;
        return playerTransform;
    }
    public SlotManager GetPlayerSlotManager(int index)
    {
        SlotManager playerSlotManager = GameManager.instance.Players[index].GetComponent<SlotManager>();
        return playerSlotManager;
    }
    private string GetStateString()
    {
        string state = "NONE";
        if (CurrentState != null)
        {
            if (CurrentState.ID == FSMStateID.Dead)
            {
                state = "DEAD";
            }
            else if (CurrentState.ID == FSMStateID.Idle)
            {
                state = "IDLE";
            }
            else if (CurrentState.ID == FSMStateID.Chasing)
            {
                state = "CHASE";
            }
            else if (CurrentState.ID == FSMStateID.MeleeAttacking)
            {
                state = "MELEEATTACK";
            }
            else if (CurrentState.ID == FSMStateID.Charging)
            {
                state = "CHARGE";
            }
        }

        return state;
    }

    // Initialize the FSM for the NPC adolescent.
    protected override void Initialize()
    {
        navAgent = this.GetComponent<NavMeshAgent>();
        health = this.GetComponent<Health>();

        rigBody = GetComponent<Rigidbody>();

        receivedAttackCommand = false;

        // Create the FSM for the adolescent.
        ConstructFSM();

    }

    // Update each frame.
    protected override void FSMUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.Reason();
            CurrentState.Act();
        }
    }

    protected override void FSMFixedUpdate()
    {

    }

    private void ConstructFSM()
    {
        //
        // TODO: Build your FSM with states here...
        //

        AdolescentChaseState chase = new AdolescentChaseState(pointList, this);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        chase.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        chase.AddTransition(Transition.InDanger, FSMStateID.Fleeing);
        chase.AddTransition(Transition.ReachPlayer, FSMStateID.MeleeAttacking);
        chase.AddTransition(Transition.Enable, FSMStateID.Idle);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        AdolescentMeleeAttackState meleeAttack = new AdolescentMeleeAttackState(pointList, this);
        meleeAttack.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        meleeAttack.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        meleeAttack.AddTransition(Transition.InDanger, FSMStateID.Fleeing);
        meleeAttack.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        meleeAttack.AddTransition(Transition.Enable, FSMStateID.Idle);
        meleeAttack.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        AdolescentIdleState idle = new AdolescentIdleState(pointList, this);
        idle.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        idle.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        idle.AddTransition(Transition.InDanger, FSMStateID.Fleeing);

        AdolescentChargeState charge = new AdolescentChargeState(pointList, this);
        charge.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        charge.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        charge.AddTransition(Transition.Enable, FSMStateID.Idle);
        charge.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        AdolescentDeadState dead = new AdolescentDeadState();

        //Add states to our state list
        AddFSMState(chase);
        AddFSMState(meleeAttack);
        AddFSMState(idle);
        AddFSMState(charge);
        AddFSMState(dead);

    }

    private void OnReceiveAttackCommand()
    {
        if (CurrentStateID == FSMStateID.MeleeAttacking)
        {
            receivedAttackCommand = true;
        }
    }

    private void OnReceiveStopAttackCommand()
    {
        receivedAttackCommand = false;
    }

    private void OnEnable()
    {
        CurrentStateID = FSMStateID.Idle;

        if (navAgent)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
        }
    }
    private void OnDisable()
    {
        if (navAgent && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
    }
}
