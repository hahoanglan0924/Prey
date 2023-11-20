using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents;

public class HunterController : Agent
{

    [Header("Agents variables")]

    //Agents variables
    [SerializeField] private float moveSpeed = 2f;
    private Rigidbody rb;

    [Header("Envrionmnet variables")]
    //Ground Materials
    Material envMaterial;
    public GameObject env;

    public GameObject prey;
    public AgentController classObject;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
      //  envMaterial = env.GetComponent<Renderer>().material;
     }


    //Called when new simulation is called
    public override void OnEpisodeBegin()
    {
        bool distanceGood;
        Vector3 spawnLocation;
        do
        {
            //Hunter
            spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4, 4f));
            distanceGood = classObject.CheckOverlap(prey.transform.localPosition, spawnLocation, 5f);

        } while (!distanceGood);

        transform.localPosition = spawnLocation;



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




    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Prey")
        {
            AddReward(15f);
            classObject.AddReward(-13f);
         //   envMaterial.color = Color.yellow;
            classObject.EndEpisode();
            EndEpisode();

           
        }
        if (other.gameObject.tag == "wall")
        {
           // envMaterial.color = Color.red;
            AddReward(-15f);
            classObject.EndEpisode();
            EndEpisode();
        }
    }

}
