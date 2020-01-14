using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Range(1, 100)] public int maxHealth = 1000;

    public int CurrentHealth;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(int inDamage)
    {
        if (CurrentHealth <= 0)
            return;

        if (CurrentHealth - inDamage >= 0)
            CurrentHealth -= inDamage;
        else if (CurrentHealth - inDamage < 0)
            CurrentHealth = 0;

    }

    public bool IsDead()
    {
        if (CurrentHealth <= 0)
            return true;

        return false;
    }
}
