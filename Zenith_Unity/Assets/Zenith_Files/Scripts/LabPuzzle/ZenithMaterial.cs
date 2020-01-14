using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZenithMaterial : MonoBehaviour
{
    public enum MaterialState { original, refined, processed, damaged }

    public GameObject completedMaterial;   
    
    public GameObject originalMaterial;
    public GameObject refinedMaterial;
    public GameObject processedMaterial;
    public GameObject damagedMaterial;

    public MaterialState currentState;

    public GameObject NextState()
    {
        switch (currentState)
        {
            case MaterialState.original:
                currentState++;
                return refinedMaterial;

            case MaterialState.refined:
                currentState++;
                return processedMaterial;

            case MaterialState.processed:
                currentState = MaterialState.damaged;
                return damagedMaterial;

            case MaterialState.damaged:
                return damagedMaterial;

            default:
                return damagedMaterial;
        }
    }       
}
