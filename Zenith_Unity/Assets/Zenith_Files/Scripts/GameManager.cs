using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Members    
    [Header("Player/Enemy Prefabs: ", order = 0)]
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;
    public GameObject runtPrefab;
    public GameObject adolescentPrefab;

    static public GameManager instance = null;
    //public GameObject playerPrefab;
    #endregion

    #region Private Members
    List<GameObject> runts;
    List<GameObject> adolescents;

    public List<GameObject> Players { get; set; }

    public int RuntCount { get; set; }
    public int AdolescentCount { get; set; }
    #endregion

    // Awake is called when the object becomes active
    private void Awake()
    {
        //check if there is already an instance of the game manager
        if (instance == null)
            instance = this;
        else if (instance != this)  //if this isn't the instance then destroy
            Destroy(gameObject);

        Players = new List<GameObject>();
        foreach (GameObject playerSpawn in GameObject.FindGameObjectsWithTag("PSP"))
        {
            if (Players.Count == 0)
            {
                GameObject playerOne = GameObject.Instantiate(playerOnePrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);

                playerOne.GetComponent<PlayerController>().PlayerNumber = Players.Count + 1;
                playerOne.transform.position = playerSpawn.transform.position;
                Players.Add(playerOne);
            }
            else if (Players.Count == 1)
            {
                GameObject playerTwo = GameObject.Instantiate(playerTwoPrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);

                playerTwo.GetComponent<PlayerController>().PlayerNumber = Players.Count + 1;
                playerTwo.transform.position = playerSpawn.transform.position;
                Players.Add(playerTwo);
            }

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        runts = new List<GameObject>();
        adolescents = new List<GameObject>();

        SpawnEnemies();

    }

    // Update is called once per frame
    void Update()
    {
        RuntCount = GameObject.FindGameObjectsWithTag("Runt").Length;
        AdolescentCount = GameObject.FindGameObjectsWithTag("Adolescent").Length;

    }

    void SpawnEnemies()
    {
        List<Transform> wayPoints = new List<Transform>();
        foreach (GameObject wp in GameObject.FindGameObjectsWithTag("WayPoint"))
        {
            wayPoints.Add(wp.transform);
        }
        List<Transform> fleePoints = new List<Transform>();
        foreach (GameObject fp in GameObject.FindGameObjectsWithTag("FleePoint"))
        {
            fleePoints.Add(fp.transform);
        }

        foreach (GameObject rsp in GameObject.FindGameObjectsWithTag("S1SP"))
        {
            runtPrefab.GetComponent<NPCRuntController>().pointList = wayPoints.ToArray();
            runtPrefab.GetComponent<NPCRuntController>().fleePoints = fleePoints.ToArray();
            runts.Add(Instantiate(runtPrefab, rsp.transform.position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f)));
        }
        foreach (GameObject asp in GameObject.FindGameObjectsWithTag("S2SP"))
        {
            adolescentPrefab.GetComponent<NPCAdolescentController>().pointList = wayPoints.ToArray(); 
            adolescentPrefab.GetComponent<NPCAdolescentController>().fleePoints = fleePoints.ToArray();
            adolescents.Add(Instantiate(adolescentPrefab, asp.transform.position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f)));

        }
    }
}
