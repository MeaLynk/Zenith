//------------------------------------------------------------------------------------------------------
// Name: AdolescentAttack (Script)
// Author: Dalton Morris
// Purpose: This class is used to define what will happen when an adolescent attacks.
//------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AI;

public class AdolescentAttack : MonoBehaviour
{
    #region Public Members    
    [Header("Properties: ", order = 0)]
    [Range(1, 100)] public int meleeDamage = 5;                 //The amount of damage the adolescent's melee attack does
    [Range(0.1f, 5.0f)] public float meleeAttackRate = 0.5f;    //How often the adolescent's will do a melee attack
    [Range(1, 100)] public int rangedDamage = 10;               //The amount of damage the adolescent's ranged attack does
    #endregion

    #region Private Members
    float elapsedTime;                                          //The elapsed time since the adolescent attacked
    List<GameObject> players;                                   //The player's GameObjects
    NPCAdolescentController adolescentController;               //NPCAdolescentController script attached to object
    Health adolescentHealth;                                    //AdolescentHealth script attached to object
    #endregion

    //GET and SET for whether or not the adolescent is attacking
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
        adolescentController = this.GetComponent<NPCAdolescentController>();
        adolescentHealth = this.GetComponent<Health>();
        Attacking = false;
    }

    //------------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void Update()
    {
        if (Attacking && !adolescentHealth.IsDead() && adolescentController.CurrentStateID == FSMStateID.MeleeAttacking)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > meleeAttackRate)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (i == adolescentController.PlayerTarget)
                        players[i].GetComponent<Health>().DealDamage(meleeDamage);

                }
                elapsedTime = 0.0f;
            }

        }

    }


}
