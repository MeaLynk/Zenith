//------------------------------------------------------------------------------------------------------
// Name: RuntAttack (Script)
// Author: Dalton Morris
// Purpose: This class is used to define what will happen when an runt attacks.
//------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AI;

public class RuntAttack : MonoBehaviour
{
    #region Public Members    
    [Header("Properties: ", order = 0)]
    [Range(1, 100)] public int meleeDamage = 5;                 //The amount of damage the runt's melee attack does
    [Range(0.1f, 5.0f)] public float meleeAttackRate = 0.5f;    //How often the runt's will do a melee attack
    [Range(1, 100)] public int rangedDamage = 10;               //The amount of damage the runt's ranged attack does
    #endregion

    #region Private Members
    float elapsedTime;                                          //The elapsed time since the runt attacked
    List<GameObject> players;                                   //The player's GameObjects
    NPCRuntController runtController;                           //RuntController script attached to object
    Health runtHealth;                                          //RuntHealth script attached to object
    #endregion

    //GET and SET for whether or not the runt is attacking
    public bool Attacking { get; set; }

    //------------------------------------------------------------------------------------------------------
    // Start is called before the first frame update
    void Start()
    {
        elapsedTime = 0.0f;
        players = new List<GameObject>();
        foreach (GameObject player in GameManager.instance.Players)
        {
            players.Add(player);
        }
        runtController = this.GetComponent<NPCRuntController>();
        runtHealth = this.GetComponent<Health>();
        Attacking = false;
    }

    //------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void Update()
    {
        if (Attacking && !runtHealth.IsDead() && runtController.CurrentStateID == FSMStateID.MeleeAttacking)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > meleeAttackRate)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (i == runtController.PlayerTarget)
                        players[i].GetComponent<Health>().DealDamage(meleeDamage);

                }
                elapsedTime = 0.0f;
            }

        }

    }


}
