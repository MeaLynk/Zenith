using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Members
    public int runtCount = 0;
    public int adolescentCount = 0;

    [Header("Hallway Triggers: ", order = 0)]
    public BoxCollider[] labTriggers;
    public BoxCollider[] engineBayTriggers;
    public BoxCollider[] refineryTriggers;

    [Header("Player/Enemy Prefabs: ", order = 1)]
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;
    public GameObject runtPrefab;
    public GameObject adolescentPrefab;

    static public GameManager instance = null;
    //public GameObject playerPrefab;
    #endregion

    #region Private Members
    GameObject lab;
    GameObject engineBay;
    GameObject refinery;

    List<GameObject> runts;
    List<GameObject> adolescents;

    bool beenInLab;
    bool beenInEngineBay;
    bool beenInRefinery;

    public List<GameObject> Players { get; set; }

    public enum Location { NONE = 0, LAB, REFINERY, ENGINE_BAY }
    List<Location> playerLocations;

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
        playerLocations = new List<Location>();
        foreach (GameObject playerSpawn in GameObject.FindGameObjectsWithTag("PSP"))
        {
            if (Players.Count == 0)
            {
                GameObject playerOne = GameObject.Instantiate(playerOnePrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);

                playerOne.GetComponent<PlayerController>().PlayerNumber = Players.Count + 1;
                playerOne.transform.position = playerSpawn.transform.position;
                Players.Add(playerOne);
                playerLocations.Add(Location.LAB);
            }
            else if (Players.Count == 1)
            {
                GameObject playerTwo = GameObject.Instantiate(playerTwoPrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);

                playerTwo.GetComponent<PlayerController>().PlayerNumber = Players.Count + 1;
                playerTwo.transform.position = playerSpawn.transform.position;
                Players.Add(playerTwo);
                playerLocations.Add(Location.LAB);
            }

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        lab = GameObject.FindGameObjectWithTag("Laboratory");
        engineBay = GameObject.FindGameObjectWithTag("EngineBay");
        refinery = GameObject.FindGameObjectWithTag("Refinery");

        runts = new List<GameObject>();
        adolescents = new List<GameObject>();

        beenInLab = true;
        beenInRefinery = false;
        beenInEngineBay = false;

        ToggleRooms();

    }

    // Update is called once per frame
    void Update()
    {
        RuntCount = GameObject.FindGameObjectsWithTag("Runt").Length;
        AdolescentCount = GameObject.FindGameObjectsWithTag("Adolescent").Length;

        runtCount = RuntCount;
        adolescentCount = AdolescentCount;

        for (int i = 0; i < Players.Count; i++)
        {
            foreach (BoxCollider lt in labTriggers)
            {
                if (lt.bounds.Contains(Players[i].transform.position) && playerLocations[i] != Location.LAB)
                {
                    if (!beenInLab)
                    {
                        SpawnEnemies(lab);
                        beenInLab = true;
                    }

                    playerLocations[i] = Location.LAB;
                    ToggleRooms();

                }
            }
            foreach (BoxCollider rt in refineryTriggers)
            {
                if (rt.bounds.Contains(Players[i].transform.position) && playerLocations[i] != Location.REFINERY)
                {
                    if (!beenInRefinery)
                    {
                        SpawnEnemies(refinery);
                        beenInRefinery = true;
                    }

                    playerLocations[i] = Location.REFINERY;
                    ToggleRooms();

                }
            }
            foreach (BoxCollider ebt in engineBayTriggers)
            {
                if (ebt.bounds.Contains(Players[i].transform.position) && playerLocations[i] != Location.ENGINE_BAY)
                {
                    if (!beenInEngineBay)
                    {
                        SpawnEnemies(engineBay);
                        beenInEngineBay = true;
                    }

                    playerLocations[i] = Location.ENGINE_BAY;
                    ToggleRooms();

                }
            }
        }
    }

    void ToggleRooms()
    {
        bool inLab = false;
        bool inRefinery = false;
        bool inEngineBay = false;

        for (int x = 0; x < playerLocations.Count; x++)
        {
            if (playerLocations[x] == Location.LAB)
                inLab = true;
            if (playerLocations[x] == Location.REFINERY)
                inRefinery = true;
            if (playerLocations[x] == Location.ENGINE_BAY)
                inEngineBay = true;
        }

        if (!refinery.activeInHierarchy)
            refinery.SetActive(true);

        if ((!inEngineBay && !inRefinery) && engineBay.activeInHierarchy)
            engineBay.SetActive(false);
        else if ((inEngineBay || inRefinery) && !engineBay.activeInHierarchy)
            engineBay.SetActive(true);

        if ((!inLab && !inRefinery) && lab.activeInHierarchy)
            lab.SetActive(false);
        else if ((inLab || inRefinery) && !lab.activeInHierarchy)
            lab.SetActive(true);

    }

    void SpawnEnemies(GameObject level)
    {
        Transform[] runtSpawnPoints = level.transform.Find("SpawnPoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "S1SP").ToArray();
        Transform[] runtWayPoints = level.transform.Find("WayPoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "WayPoint").ToArray();
        Transform[] runtFleePoints = level.transform.Find("FleePoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "FleePoint").ToArray();

        for (int i = 0; i < runtSpawnPoints.Count(); i++)
        {
            runtPrefab.GetComponent<NPCRuntController>().pointList = runtWayPoints;
            runtPrefab.GetComponent<NPCRuntController>().fleePoints = runtFleePoints;
            runts.Add(Instantiate(runtPrefab, runtSpawnPoints[i].position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), level.transform));

        }

        Transform[] adolescentSpawnPoints = level.transform.Find("SpawnPoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "S2SP").ToArray();
        Transform[] adolescentWayPoints = level.transform.Find("WayPoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "WayPoint").ToArray();
        Transform[] adolescentFleePoints = level.transform.Find("FleePoints").transform.Cast<Transform>().Where(child => child.gameObject.tag == "FleePoint").ToArray();

        for (int x = 0; x < adolescentSpawnPoints.Count(); x++)
        {
            adolescentPrefab.GetComponent<NPCAdolescentController>().pointList = adolescentWayPoints;
            adolescentPrefab.GetComponent<NPCAdolescentController>().fleePoints = adolescentFleePoints;
            adolescents.Add(Instantiate(adolescentPrefab, adolescentSpawnPoints[x].position, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), level.transform));

        }
    }

}
