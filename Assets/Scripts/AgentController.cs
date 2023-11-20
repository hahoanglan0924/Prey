using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators; //Move(Output)
using Unity.MLAgents.Sensors; //Sensing(Input)
using static Unity.VisualScripting.Metadata;

public class AgentController : Agent
{
    [Header("Coin variables")]
    //Able to see private variable from inspector
    public int coinCount;
    public GameObject coin;
    [SerializeField] private List<GameObject> spawnedCoinsList = new List<GameObject>();

    [Header("Agents variables")]

    //Agents variables
    [SerializeField] private float moveSpeed = 2f;
    private Rigidbody rb;

    [Header("Envrionmnet variables")]
    [SerializeField] private Transform environmentLocation;
    
    
    //Ground Materials
    Material envMaterial;
    public GameObject env;

    Color failedMaterial;
    Color passMaterial;
    Color timePassed;

    [Header("Time keeping variables")]
    [SerializeField] private int timeForEpisode;
    private float timeLeft;


    [Header("Hunter Agent")]
    public HunterController classObject;

    //Start of the training
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
        // Create materials with specific colors
        
        failedMaterial = Color.red;

        passMaterial = Color.green;

        timePassed = Color.blue;
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    //Called when new simulation is called
    public override void OnEpisodeBegin()
    {
        //Agents location
        transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.25f, Random.Range(-4,4f)); CreatePellet();

        EpisodeTimerNew();
    }

    private void CreatePellet()
    {
           if(spawnedCoinsList.Count != 0)
        {
            RemoveCoin(spawnedCoinsList);
        }

        for(int i = 0; i < coinCount; i++)
        {
            //Spawning pellet
            GameObject newCoin = Instantiate(coin);
            //Make pellet child of the envrionment
            newCoin.transform.parent = environmentLocation;
            Vector3 pelletLocation = new Vector3(Random.Range(-7f, 7f), 0.25f, Random.Range(-7, 7f));
            newCoin.transform.localPosition = pelletLocation;
            //Add to list
            spawnedCoinsList.Add(newCoin);
        }
    }
    public List<float> distanceList = new List<float>();
    public List<float> badDistanceList = new List<float>();

    public bool CheckOverlap(Vector3 objectWeWantToAvoidOverlapping, Vector3 alreadyExistingObject, float minDistanceWanted)
    {
        float DistanceBetweenObjects = Vector3.Distance(objectWeWantToAvoidOverlapping, alreadyExistingObject);
        if(minDistanceWanted <= DistanceBetweenObjects)
        {
            distanceList.Add(DistanceBetweenObjects);
            return true;
        }
        badDistanceList.Add(DistanceBetweenObjects);
        return false;
    }

    private void RemoveCoin(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Allow agents to know where they are
        sensor.AddObservation(transform.localPosition);
     //   sensor.AddObservation(target.localPosition);
    }

    //Feed human input
    public override void Heuristic(in ActionBuffers actions)
    {
        ActionSegment<float> conitnuesActions = actions.ContinuousActions;
        conitnuesActions[0] = Input.GetAxisRaw("Horizontal");
        conitnuesActions[1] = Input.GetAxisRaw("Vertical");
    }

    //Move
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);

    }

    //Reward judge
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "coin")
        {
           //Remove from list
           spawnedCoinsList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            if(spawnedCoinsList.Count <= 0)
            {
                envMaterial.color = passMaterial;
                RemoveCoin(spawnedCoinsList);
                AddReward(5f);
                classObject.AddReward(-5f);
                classObject.EndEpisode();
                EndEpisode();
            }
        }
        if(other.gameObject.tag == "wall")
        {
            envMaterial.color = failedMaterial;
            AddReward(-15f);
            RemoveCoin(spawnedCoinsList);
            classObject.EndEpisode();
            EndEpisode();
        }
    }


    private void EpisodeTimerNew()
    {
        timeLeft = Time.time + timeForEpisode;
    }

    private void CheckRemainingTime()
    {
        if (Time.time > timeLeft)
        {
            envMaterial.color = timePassed;
            AddReward(-15f);
            classObject.AddReward(-15f);
            RemoveCoin(spawnedCoinsList);
            classObject.EndEpisode();
            EndEpisode();
        }
    }
}

