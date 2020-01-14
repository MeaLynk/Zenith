using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabMachine : MonoBehaviour
{
    public enum MachineType { centrifuge, compactor, combinator }

    public MachineType machineType;

    public Transform machineOutput;

    public AudioClip machineIdleSound;
    public AudioClip machineAltSound;
    public AudioClip machineOperatingSound;

    public float audioInterval;
    float elapsedTime;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject newMaterial = ProcessMaterial(collision.gameObject, machineType);

        if (newMaterial != null)
        {
            //Play AudioClip for machine operating and or particle effects
            Destroy(collision.gameObject);
            Instantiate<GameObject>(newMaterial, machineOutput);
        }
    }

    /// <summary>
    /// Functionality of the machines
    /// </summary>
    /// <param name="assetIn">The game object placed into the machine</param>
    /// <param name="machine">The machine being used</param>
    /// <returns></returns>
    public GameObject ProcessMaterial(GameObject assetIn, MachineType machine)
    {
        ZenithMaterial objIn;

        if (assetIn.TryGetComponent<ZenithMaterial>(out objIn))
        {
            if ((int)objIn.currentState == (int)machine)
            {
                //Material was placed in the proper machine
                return objIn.NextState();
            }
            else
            {
                //Material was placed in the wrong machine
                return objIn.damagedMaterial;
            }
        }
        return null;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > audioInterval)
        {
            elapsedTime = 0;

            //Play Machine alt sound and or particle effects
        }
    }
}
