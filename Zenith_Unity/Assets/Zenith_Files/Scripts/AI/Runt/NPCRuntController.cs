﻿//NPCRuntController
//This class is derived from AdvancedFSM and holds the FSM for the NPC Runt
//each runt must have this attached to it in order to have "runt" behaviour
//
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NPCRuntController : AdvancedFSM
{
    public GameObject destination;
    public static int SLOT_DIST = 3;
    public static int ATTACK_DIST = 3;
    public static int RANGED_ATTACK_DIST = 12;
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
        return GameManager.instance.Players[index].transform;
    }

    public SlotManager GetPlayerSlotManager(int index)
    {
        return GameManager.instance.Players[index].GetComponent<SlotManager>();
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
            else if (CurrentState.ID == FSMStateID.RangedAttacking)
            {
                state = "RANGEDATTACK";
            }
            else if (CurrentState.ID == FSMStateID.Fleeing)
            {
                state = "FLEE";
            }
        }

        return state;
    }

    // Initialize the FSM for the NPC runt.
    protected override void Initialize()
    {
        navAgent = this.GetComponent<NavMeshAgent>();
        health = this.GetComponent<Health>();
        rigBody = this.GetComponent<Rigidbody>();

        receivedAttackCommand = false;

        // Create the FSM for the runt.
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
        RuntChaseState chase = new RuntChaseState(pointList, this);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        chase.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        chase.AddTransition(Transition.InDanger, FSMStateID.Fleeing);
        chase.AddTransition(Transition.ReachPlayer, FSMStateID.MeleeAttacking);
        chase.AddTransition(Transition.Enable, FSMStateID.Idle);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        RuntRangedAttackState rangedAttack = new RuntRangedAttackState(pointList, this);
        rangedAttack.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        rangedAttack.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        rangedAttack.AddTransition(Transition.InDanger, FSMStateID.Fleeing);
        rangedAttack.AddTransition(Transition.Enable, FSMStateID.Idle);
        rangedAttack.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        RuntMeleeAttackState meleeAttack = new RuntMeleeAttackState(pointList, this);
        meleeAttack.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        meleeAttack.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        meleeAttack.AddTransition(Transition.InDanger, FSMStateID.Fleeing);
        meleeAttack.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        meleeAttack.AddTransition(Transition.Enable, FSMStateID.Idle);
        meleeAttack.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        RuntIdleState idle = new RuntIdleState(pointList, this);
        idle.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        idle.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        idle.AddTransition(Transition.InDanger, FSMStateID.Fleeing);

        RuntFleeState flee = new RuntFleeState(fleePoints, this);
        flee.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        flee.AddTransition(Transition.PlayerDead, FSMStateID.Idle);
        flee.AddTransition(Transition.ReachPlayer, FSMStateID.RangedAttacking);
        flee.AddTransition(Transition.Enable, FSMStateID.Idle);
        flee.AddTransition(Transition.LostPlayer, FSMStateID.Idle);

        RuntDeadState dead = new RuntDeadState();

        //Add states to the state list
        AddFSMState(chase);
        AddFSMState(rangedAttack);
        AddFSMState(meleeAttack);
        AddFSMState(idle);
        AddFSMState(flee);
        AddFSMState(dead);

    }

    private void OnReceiveAttackCommand()
    {
        if (CurrentStateID == FSMStateID.MeleeAttacking || CurrentStateID == FSMStateID.RangedAttacking)
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